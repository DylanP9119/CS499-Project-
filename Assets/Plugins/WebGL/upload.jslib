mergeInto(LibraryManager.library, {
  ShowFileUpload: function () {
    var input = document.createElement('input');
    input.type = 'file';
    input.accept = '.json';

    input.onchange = (event) => {
      var file = event.target.files[0];
      var reader = new FileReader();

      reader.onload = function (e) {
        var json = e.target.result;

        // Send the JSON string to Unity
        SendMessage('UILoadMenuController', 'OnJsonFileLoaded', json);
      };

      reader.readAsText(file);
    };

    input.click();
  }
});
