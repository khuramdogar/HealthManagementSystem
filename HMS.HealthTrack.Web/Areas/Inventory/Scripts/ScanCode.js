var spcBox, upcBox, upcGrid, productId;

function createScanCodeWindow(spc, upc,scanCodeproductId, grid) {
   if ($('#scanCodeWindow').data('kendoWindow')) return;

   spcBox = spc;
   upcBox = upc;
   upcGrid = grid;
   productId = scanCodeproductId;
   configureScanWindow();
}

function configureScanWindow() {
   $('#scanCodeWindow').kendoWindow({
      content: '/Inventory/ScanCode/ScanCodeWindow',
      modal: true,
      resizable: false,
      scrollable: false,
      title: 'Scan barcode',
      visible: false,
      width: 450,

      activate: function (e) {
         $('#ScanCode').keypress(function (ee) {
            var keyCode = ee.KeyCode ? ee.keyCode : ee.which;
            if (keyCode != 13) return;

            if ($('#ScanCode').val() == '') {
               return;
            }

            onSearchScanCodeWindowClick();
         });
         $('#ScanCode').focus();
      },

      open: function (e) {
         $('#ScanCode').val('');
         $('#scanAlert').empty();
      }
   });
}

function addUpnToGrid(grid,upnValue,productId) {
   var gridData = grid.data("kendoGrid");
   var scanCode = { ScanCodeId: null, ProductId: productId, Value: upnValue };
   var gridItem = gridData.dataSource.insert(scanCode);
   gridItem.set("dirty", true);
}

function onCloseScanCodeWindowClick(e) {
   $('#ScanCode').val('');
   $('#scanCodeWindow').data('kendoWindow').close();
}

function onSearchScanCodeWindowClick(e) {
   var scanCodeValue = $('#ScanCodeValue').val();
   var code = $('#ScanCode').val();

   $.ajax({
      async: false,
      contentType: 'application/json',
      data: JSON.stringify({
         scanCodeValue: scanCodeValue,
         code: code
      }),
      type: 'POST',
      url: '/api/Inventory/ScanCode/ExtractCodeValue',
      success: function(data) {
         switch (scanCodeValue) { // SEE ScanCodeValue enum
            case 'Spc': // Spc
               var spc = $('#' + spcBox);
               spc.val(data);
               $('#alertPlaceholder').empty();
               setTimeout(function () {
                  spc.fadeOut(300).fadeIn(300).css("background-color", "#dff0d8").css("border-color", '#d6e9c6').css('color', '#3c763d');
               }, 500);

               setTimeout(function () {
                  spc.css('background-color', '#fff').css('border-color', '#d5d5d5').css('color', '#5f2200');
               }, 2000);
            break;
            case 'Upc': // Upc
               var upn = $('#' + upcBox);
               upn.val(data);

               if (upcGrid != null && upcGrid.length) {
                  addUpnToGrid(upcGrid, data,productId);
               }

               $('#alertPlaceholder').empty();
               setTimeout(function() {
                  upn.fadeOut(300).fadeIn(300).css("background-color", "#dff0d8").css("border-color", '#d6e9c6').css('color', '#3c763d');
               }, 500);

               setTimeout(function() {
                  upn.css('background-color', '#fff').css('border-color', '#d5d5d5').css('color', '#5f2200');
               }, 2000);
         default:
         }
         onCloseScanCodeWindowClick();
      },
      error: function (data) {
         $('#ScanCode').focus().select();
         if (data.responseJSON != undefined) {
            getAlertForId('scanAlert', 'danger', data.responseJSON.Message);
            return;
         }
         getAlertForId('scanAlert', 'danger', 'An error occurred while extracting the value from the barcode.');
      }
   });
}
