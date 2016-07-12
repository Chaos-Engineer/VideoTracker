//
// Script for the phantomjs headless browser to bypass CloudFlare DDoS protection.
//
// This version simply waits until the "loadingString" page no longer appears, and the
// page contents have remained unchanged for the past 500 milliseconds. (The second check is to
// ensure that the page is no longer being updated.)
//
// PhantomJS returns a <TITLE>Error</TITLE> page for errors that it detects. We use the same
// format for errors detected within this script.
//
var loadingString  = "DDoS protection by CloudFlare";
var timeoutSeconds = 60;
var oldContent = "";

var page = require('webpage').create();
var system = require('system');

window.setTimeout(forceExit, timeoutSeconds * 1000);
page.onError = errorHandler;
page.open(system.args[1], openCompletion);

function openCompletion(status) {
  if (status !== "success") {
      console.log("<TITLE>CloudFlareBypass Error</TITLE>Initial load failed");
      phantom.exit(0);
  }
  checkPageLoaded();
}

function checkPageLoaded() {
    var content = page.content;
    if (content.indexOf(loadingString) > -1 || content !== oldContent) {
       oldContent = content;
       window.setTimeout(checkPageLoaded,500);
       return;
    }
    console.log(page.content.replace(/\n|\r/g, ""));
    phantom.exit(0);
}

function forceExit() {
  console.log("<TITLE>CloudFlareBypass Error</TITLE>Unable to load page in " + timeoutSeconds + " seconds.");
  phantom.exit(0);
}

// Ignore errors caused by things like advertising javascript failing.
function errorHandler(msg, trace) {
    var msgStack = ['ERROR: ' + msg];
    if (trace && trace.length) {
        msgStack.push('TRACE:');
        trace.forEach(function(t) {
            msgStack.push(' -> ' + t.file + ': ' + t.line + (t.function ? ' (in function "' + t.function + '")' : ''));
        });
    }
    // Uncomment to log into the console.
    //console.log(msgStack.join('\n'));
}
