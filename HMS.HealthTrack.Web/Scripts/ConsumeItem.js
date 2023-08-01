function onSerialNumberChange() {
   var quantity = $('#Quantity');
   if (this.value().trim() != '') {
      quantity.attr('readonly', 'readonly');
      quantity.val(1);
   } else if (!quantity.is('[requiresSerial]')) {
      quantity.removeAttr('readonly');
   }
}

function onProductSelect(e) {
   var selected;
   var index = e.item.index();
   if (index == 0 && !e.item.is("li")) {
      selected = "";
   } else {
      selected = this.dataItem(++index).Value;
   }

   $('#ProductId').val(selected);
   if (selected == "") return;

   setProductDetails(selected);
   $('#UPNSearch').data('kendoComboBox').value('');
}

function filterSerialNumbers() {
   return {
      productId: $('#ProductId').val(),
      locationId: $('#StockLocationId').val()
   }
}

function onLocationChange(e) {
   if (this.value() && this.selectedIndex == -1) {
      var dt = this.dataSource._data[0];
      this.text(dt[this.options.dataTextField]);
      this._selectItem();
   } else {
      $('#SerialNumber').data('kendoComboBox').dataSource.read();
   }
}

function onLocationDataBound(e) {
   this.value($('#UserPreferredLocation').val());
}