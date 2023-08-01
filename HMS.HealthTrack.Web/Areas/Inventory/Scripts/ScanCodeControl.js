var allowFilter = false;

function initialiseScanCodeControl() {
   var codeBox = $('#Code').data('kendoComboBox');
   codeBox.focus();
   $('#Code').data('kendoComboBox').input.on('keypress', function (e) {
      var code = e.keyCode ? e.keyCode : e.which;
      if (code == 13) {
         $('#alertPlaceholder').empty();
         resetProduct();
         allowFilter = true;
         var codeComboBox = $('#Code').data('kendoComboBox');
         codeComboBox.search(codeComboBox.value());
         e.preventDefault();
      }
   });
}

function onCodeDataBound(e) {
   var itemCount = e.sender.dataSource.total();

   if (itemCount == 1) {
      if (e.sender._state == "rebind") {
         
      } else {
         e.sender.select(0);
         e.sender.close();
         e.sender.trigger('select');
      }
   }
}

function onCodeFiltering(e) {
   if (allowFilter == false) {
      e.preventDefault();
      return;
   }
   allowFilter = false;
}

function onCodeOpen(e) {
   $('#alertPlaceholder').empty();
   allowFilter = true;
   var codeComboBox = $('#Code').data('kendoComboBox');
   codeComboBox.search(codeComboBox.value());
}

function onCodeSelect(e) {
   var createNewProductMessage = 'No product was found for this code. Would you like to <a href="javascript:void(0);" onclick="showQuickCreateWindow();">' +
      'create a new product</a>?';

   var toSelect;
   if (e.item == undefined) {
      toSelect = 0;
   } else {
      toSelect = e.item;
   }
   var data = e.sender.dataItem(toSelect).Value;
   if (data.ProductId == null || data.ProductId == '' || data.ProductId == 0) {

      getAlert('warning', createNewProductMessage);
      $('#QuickCreateSpc').val(data.SPC);
      $('#QuickCreateUpn').val(data.UPN);
      return;
   }

   $('#ProductId').val(data.ProductId);
   $('#productDescription').text(data.Description);
   $('#productSPC').text(data.SPC);
   $('#productUPN').text(data.UPN);
   $('#productManageStock').val(data.ManageStock);
   showProductInformation();
}

function resetProduct() {
   $('#ProductId').val(0);
   $('#productDescription').text('');
   $('#productSPC').text('');
   $('#productUPN').text('');
   $('#QuickCreateDescription').val('');
   $('#QuickCreateSpc').val('');
   $('#QuickCreateUpn').val('');
}