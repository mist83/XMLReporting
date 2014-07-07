using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MediaForge3.Common.DataContracts.TMS.Programs;

namespace ConsoleApplication1
{
    public static class SchemaGenerator
    {
        private static int indentAmount = 4;
        private static string stagingTablePrefix = "_";

        public static HashSet<string> GenerateSQL(Type type, int indentLevel, params string[] nonLinkedTableNames)
        {
            HashSet<string> sql = new HashSet<string>();
            var properties = type.GetProperties().OrderBy(x => x.Name).Reverse();

            var classProperties = properties.Where(x =>
                x.PropertyType != typeof(string) &&
                x.PropertyType != typeof(decimal) &&
                x.PropertyType != typeof(DateTime) &&
                x.PropertyType != typeof(bool) &&
                x.PropertyType != typeof(float));

            List<PropertyInfo> childLinkingProperties = new List<PropertyInfo>();
            if (classProperties.Any())
            {
                foreach (var item in classProperties)
                {
                    // Add properties that will be picked up when the table is generated that links this record to the one below
                    childLinkingProperties.AddRange(item.PropertyType.GetProperties().Where(x => x.Name.EndsWith("Id")));

                    var subProperties = GenerateSQL(item.PropertyType.GenericTypeArguments.Any() ? item.PropertyType.GenericTypeArguments.Single() : item.PropertyType, indentLevel + 1, nonLinkedTableNames);
                    foreach (var subTable in subProperties)
                    {
                        if (sql.Any(x => x.Trim() == subTable.Trim()))
                            sql.Add("--" + subTable);
                        else
                            sql.Add(subTable);
                    }
                }
            }

            var simpleProperties = properties.Union(childLinkingProperties).Except(classProperties).Where(x => !x.PropertyType.GenericTypeArguments.Any());
            if (simpleProperties.Any())
            {
                if (nonLinkedTableNames.Contains(type.Name))
                {
                    var sqlStatement = string.Format("{0}CREATE TABLE [dbo].[{1}{2}] ({3})", string.Empty.PadLeft(indentLevel * indentAmount, ' '), stagingTablePrefix, type.Name, string.Join(", ",
                        simpleProperties
                        .OrderBy(x => x.Name == "TMSId" ? 0 : 1)
                        .ThenBy(x => x.Name.EndsWith("Id") ? 0 : 1)
                        .ThenBy(x => x.Name)
                        .Select(x => string.Format("[{0}] {1}", x.Name, ConvertTypeToSQLType(x.PropertyType)))));

                    sql.Add(sqlStatement);
                }
                else
                {
                    string sqlStatement = string.Format("{0}CREATE TABLE [dbo].[{1}{2}] ([TMSId] varchar(max), {3})", string.Empty.PadLeft(indentLevel * indentAmount, ' '), stagingTablePrefix, type.Name, string.Join(", ",
                        simpleProperties
                        .OrderBy(x => x.Name.EndsWith("Id") ? 0 : 1)
                        .Select(x => string.Format("[{0}] {1}", x.Name, ConvertTypeToSQLType(x.PropertyType)))));

                    sql.Add(sqlStatement);
                }
            }

            return sql;
        }

        static string ConvertTypeToSQLType(Type type)
        {
            if (type == typeof(int))
                return "int";
            else if (type == typeof(string))
                return "nvarchar(max)";
            else if (type == typeof(DateTime))
                return "datetime";
            else if (type == typeof(bool))
                return "bit";
            else if (type == typeof(Single))
                return "float";
            else if (type == typeof(Guid))
                return "uniqueidentifier";
            else
                throw new NotImplementedException();
        }

        public static IEnumerable<string> GenerateSQLInsertStatement(object item, string tmsId, params string[] excludedTables)
        {
            List<string> statements = new List<string>();

            if (item is string)
                return statements;

            var simplePropertyInfos = GetSimplePropertyInfos(item).ToList();
            List<PropertyInfo> linkingProperties = new List<PropertyInfo>();
            foreach (var classProperty in item.GetType().GetProperties().Except(simplePropertyInfos))
            {
                // Add properties that will be picked up when the table is generated that links this record to the one below
                linkingProperties.AddRange(classProperty.PropertyType.GetProperties().Where(x => x.Name.EndsWith("Id")));

                if (!classProperty.PropertyType.GenericTypeArguments.Any())
                    statements.AddRange(GenerateSQLInsertStatement(classProperty.GetValue(item), tmsId, excludedTables));
                else
                {
                    var enumerable = (IEnumerable)classProperty.GetValue(item);
                    foreach (var listItem in enumerable)
                    {
                        statements.AddRange(GenerateSQLInsertStatement(listItem, tmsId, excludedTables));
                    }
                }
            }

            if (simplePropertyInfos.Any())
            {
                if (!excludedTables.Contains(item.GetType().Name))
                {
                    if (linkingProperties.Any())
                    {
                        string sql = string.Format("INSERT {0}{1}(" + (item.GetType() == typeof(programsProgram) ? string.Empty : "[TMSId], ") + "{2}, {3}) VALUES({4} {5}, {6})",
                            stagingTablePrefix,
                            item.GetType().Name,
                            string.Join(", ", linkingProperties.Select(x => string.Format("[{0}]", x.Name))),
                            string.Join(", ", simplePropertyInfos.Select(x => string.Format("[{0}]", x.Name))),
                            (item.GetType() == typeof(programsProgram) ? string.Empty : "'" + tmsId + "', "),
                            string.Join(", ", Enumerable.Repeat("'unknown'", linkingProperties.Count)),
                            GetSqlPropertyValues(item));
                        statements.Add(sql);
                    }
                    else
                    {
                        string sql = string.Format("INSERT {0}{1}([TMSId], {2}) VALUES('{3}', {4})",
                            stagingTablePrefix,
                            item.GetType().Name,
                            string.Join(", ", simplePropertyInfos.Select(x => string.Format("[{0}]", x.Name))),
                            tmsId,
                            GetSqlPropertyValues(item));
                        statements.Add(sql);
                    }
                }
                else
                {
                    string sql = string.Format("INSERT {0}{1}({2}) VALUES({3})", stagingTablePrefix, item.GetType().Name, string.Join(", ", simplePropertyInfos.Select(x => string.Format("[{0}]", x.Name))), GetSqlPropertyValues(item));
                    statements.Add(sql);
                }
            }

            return statements;
        }

        public static IEnumerable<PropertyInfo> GetSimplePropertyInfos(object item)
        {
            var simpleProperties = item.GetType().GetProperties()
                .OrderBy(x => x.Name)
                .Where(x =>
                    x.PropertyType == typeof(int) ||
                    x.PropertyType == typeof(decimal) ||
                    x.PropertyType == typeof(string) ||
                    x.PropertyType == typeof(DateTime) ||
                    x.PropertyType == typeof(bool) ||
                    x.PropertyType == typeof(Single) ||
                    x.PropertyType == typeof(Guid) ||
                    x.PropertyType == typeof(float));

            return simpleProperties;
        }

        public static string GetSqlPropertyValues(object item)
        {
            return string.Join(", ", GetSimplePropertyInfos(item).Select(x => GetSQLPropertyValue(item, x)));
        }

        public static string GetSQLPropertyValue(object item, PropertyInfo info)
        {
            string sqlProperty = "NULL";

            if (info.PropertyType == typeof(int))
                sqlProperty = info.GetValue(item).ToString();
            if (info.PropertyType == typeof(decimal))
                sqlProperty = info.GetValue(item).ToString();
            else if (info.PropertyType == typeof(string))
                sqlProperty = string.Format("'{0}'", ((string)info.GetValue(item) ?? string.Empty).Replace("'", "''"));
            else if (info.PropertyType == typeof(DateTime))
                sqlProperty = string.Format("'{0}'", DateTime.Parse(info.GetValue(item).ToString()) < new DateTime(1753, 1, 1) ? new DateTime(1753, 1, 1) : DateTime.Parse(info.GetValue(item).ToString()));
            else if (info.PropertyType == typeof(bool))
                sqlProperty = (bool)info.GetValue(item) ? "1" : "0";
            else if (info.PropertyType == typeof(Single))
                sqlProperty = info.GetValue(item).ToString();
            else if (info.PropertyType == typeof(Guid))
                sqlProperty = string.Format("'{0}'", info.GetValue(item));

            if (sqlProperty == "''")
                sqlProperty = "NULL";

            return sqlProperty;
        }
    }
}
