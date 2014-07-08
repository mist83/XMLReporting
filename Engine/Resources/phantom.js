// Nudge in right direction: http://stefaanlippens.net/spider-javascript-manipulated-html-with-phantomjs

var fileSystem = require('fs');

// Without specifically telling PhantomJS this is a file, all "<" and ">" will be encoded
var sourceFile = "file:///" + phantom.args[0];
var destinationFile = phantom.args[1];

var page = require('webpage').create();
page.open(sourceFile, function (status) {
	if (status === 'success') {
		// Process JavaScript (if any) in the page
		var processedHTML = page.evaluate(function () {

			// Get the root node
			var html = document.getElementsByTagName('html')[0];

			// Cycle through all non-terminating tags (ones that will not conform
			// to XML - i.e. a <br /> tag is valid XHTML but not valid HTML (<br>)
			var nonTerminatingTags = html.getElementsByTagName('br');
			while (nonTerminatingTags.length > 0) {
				var newNode = document.createElement('div');
				newNode.setAttribute('data-original-type', 'br');
				nonTerminatingTags[0].parentNode.replaceChild(newNode, nonTerminatingTags[0]);
			}

			// Remove all scripts
			var scripts = html.getElementsByTagName('script');
			while (scripts.length > 0) {
				scripts[0].parentNode.removeChild(scripts[0]);
			}

			return document.documentElement.outerHTML;
		});

		// Write the HTML to the specified file
		fileSystem.write(destinationFile, processedHTML, 'w');
	}
	else {
		fileSystem.write(destinationFile, 'Could not open source file: ' + sourceFile, 'w');
	}

	// Stop PhantomJS
	phantom.exit();
});
