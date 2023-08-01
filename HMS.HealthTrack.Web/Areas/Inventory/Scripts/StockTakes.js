function createStockTakeLocationSelectionWindow() {
   $('#stockTakeLocationSelection').kendoWindow({
      content: '/Inventory/StockTakes/StockTakeLocationSelection',
      modal: true,
      resizable: false,
      title: 'Select stock take location',
      width: '500',
      visible: false,
      
      refresh: function (e) {
         $('#stockTakeLocationSelection').keypress(function (ee) {
            if (ee.keyCode == 13) {
               $('#selectLocation').trigger('click');
            }
         });
         $('#closeWindow').click(function () {
            e.sender.close();
         });
      }
   });
}