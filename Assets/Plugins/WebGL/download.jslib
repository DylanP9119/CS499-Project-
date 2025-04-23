mergeInto(LibraryManager.library, {
  DownloadFileWebGL: function (fileNamePtr, dataPtr) {
        var fileName = Pointer_stringify(fileNamePtr);
        var data = Pointer_stringify(dataPtr);
        var blob = new Blob([data], { type: 'text/plain' });
        var url = URL.createObjectURL(blob);
        var a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    }
});