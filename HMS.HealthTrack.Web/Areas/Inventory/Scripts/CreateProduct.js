var message;
function CreateProductWindow(windowMessage) {
   message = windowMessage;
   $('#CreateProductWindow').kendoWindow({
      content: '/Inventory/Products/CreateWindow',
      modal: true,
      resizable: false,
      title: 'Create a new product',
      width: '450',
      visible: false,
      refresh: onRefreshCreateProductWindow
   });
}

function openCreateProductWindow() {
   $('#CreateProductWindow').data('kendoWindow').center().open();
}

function onRefreshCreateProductWindow(e) {
   $('#cancelCreateProductWindow').click(function (buttonEvent) {
      e.sender.close();
      buttonEvent.preventDefault();
      return false;
   });
   SetProductWindowMessage(message);
}

function SetProductWindowMessage(message) {
   if (message == '') return;
   $('#createProductMessagePlaceholder').addClass('alert');
   $('#createProductMessagePlaceholder').addClass('alert-info');
   $('#createProductMessage').html(message);
}

function createQuickCreateWindow() {
   if ($('#CreateProductWindow').data('kendoWindow')) return;

   $('#CreateProductWindow').kendoWindow({
      content: '/Inventory/Products/CreateWindow',
      modal: true,
      resizable: false,
      scrollable: false,
      title: 'Quick create product',
      width: 400,
      visible: false,
      open: function (data) {
         var win = this;
         $('#cancelCreateProductWindow').click(function (e) {
            e.preventDefault();
            win.close();
         });
      },
      refresh: function (data) {
         var tabstrip = $("#tabstrip").data('kendoTabStrip');
         tabstrip.remove(1);
      }
   });
}

function showQuickCreateWindow() {
   $('#CreateProductWindow').data('kendoWindow').center().open();
}
