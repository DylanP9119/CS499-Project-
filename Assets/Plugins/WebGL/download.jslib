mergeInto(LibraryManager.library, {
  DownloadFileWebGL: function (filenamePtr, contentPtr) {
    var filename = UTF8ToString(filenamePtr);
    var content = UTF8ToString(contentPtr);

    var element = document.createElement('a');
    element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(content));
    element.setAttribute('download', filename);
    element.style.display = 'none';

    document.body.appendChild(element);
    element.click();
    document.body.removeChild(element);
  }
});