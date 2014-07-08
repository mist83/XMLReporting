var page = require('webpage').create(), fs = require('fs');

page.viewportSize = { width: 1000, height: 900 };
if (phantom.args.length === 0) {
    console.log('Usage: phantom.js <URL to render>  <FILE path for where to save the output to> <width> <height> <margin>');
    phantom.exit();
} else {
    var address = phantom.args[0];
    var fileName = phantom.args[1];
    var width = phantom.args[2];
    var height = phantom.args[3];
    //var margin = phantom.args[4];
    if (width || height) {
        page.viewportSize = { width: width, height: height };
    }
    page.onError = function (msg, trace) {
        console.error("Error: " + msg);
        trace.forEach(function (item) {
            console.log('  ', item.file, ':', item.line);
        });
    };

    page.onConsoleMessage = function (msg, lineNum, sourceId) {
        console.log('CONSOLE: ' + msg + ' (from line #' + lineNum + ' in "' + sourceId + '")');
    };

    page.open(address, function (status) {
        if (status !== 'success') {
            console.log('Unable to access HTML Page');
        }
        else {

            var markup = page.evaluate(function (margin) {
                //document.body.style.margin = margin;
                //console.log(margin);
                //$('body').css({ 'margin': margin });
                var html = document.getElementsByTagName('html')[0];

                // Remove all scripts
                var scripts = html.getElementsByTagName('script');
                console.log(scripts);
                while (scripts.length > 0) {
                    scripts[0].parentNode.removeChild(scripts[0]);
                }

                //html.innerHTML = '<div>wtf</div>';

                //document.body.style.margin = "";
                //$('body').css({ 'margin': "" });
                console.log(document.documentElement.outerHTML);
                return html.innerHTML;
            });//, margin);

            fs.write(fileName, markup, 'w');
            // console.log( markup); 
            phantom.exit();
        }
    });
}