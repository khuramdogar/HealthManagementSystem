function onRebateCodeChange(e) {
   var rebateCode = e.sender.value();
   $('#medicareProductDetails').innerHtml = "";
   if (rebateCode == "") {
      return;
   }

   $('#medicareProductDetails').load("/Inventory/MedicareProducts/Details?rebateCode=" + rebateCode);
}

function getAlert(alertType, message) {
   $('#alertPlaceholder').empty();
   $('#alertPlaceholder').append('<div id="alertdiv" class="alert alert-' + alertType + '"><a class="close" data-dismiss="alert">&times;</a><span>'
      + message
      + '</span></div>');
}

function getAlertForId(id, alertType, message) {
   $('#' + id).empty();
   $('#' + id).append('<div id="alertdiv" class="alert alert-' + alertType + '"><a class="close" data-dismiss="alert">&times;</a><span>'
      + message
      + '</span></div>');
}

function registerDelete() {
   $('#Delete').on('click', function() {
      $('#DeletePrompt').data('kendoWindow').center().open();
   });
}

function dynamicDeletePrompt(grid, description, url) {
   $('#itemDescription').text(description);
   $('#submitDelete').click(function(e) {
      e.preventDefault();
      $.ajax({
         async: false,
         type: 'DELETE',
         url: url,
         success: function() {
            getAlert('success', 'The item <strong>' + description + '</strong> has been successfully deleted.');
            $('#DeletePrompt').data('kendoWindow').close();
            grid.dataSource.read();
         },
         error: function() {
            getAlert('danger', 'There was an error deleting the item <strong>' + description + '</strong>.');
         }
      });
   });
   $('#DeletePrompt').data('kendoWindow').center().open();
}

function getItemFromDataSource(dataSource, productId) {
   var existingItems = $.grep(dataSource.data(), function(d) {
      return d.Value == productId;
   });

   if (existingItems.length == 0) {
      return null;
   }
   return existingItems[0];
}

function createCategoryTreeWindow(searchCategoriesElement, otherCategoriesElement) {
   if ($('#categoryTreeWindow').data('kendoWindow')) return;
   $('#categoryTreeWindow').kendoWindow({
      content: '/Inventory/Categories/CategorySelector',
      modal: true,
      resizable: false,
      scrollable: false,
      title: 'Select categories',
      visible: false,
      width: 450,
      // events
      activate: function() {
         var treeList = $('#CategoryTree').data('kendoTreeList');
         treeList.clearSelection();
         var toolbarTemplateString = '<span class="toolbar">' +
            '<span class="toolbar-right">' +
            '<label class="filter-label" for="Name">Name</label>' +
            '<input class="filter-control k-textbox" id="Name" name="Name">' +
            '<a class="k-button" id="applyCategoryFilter">Filter</a>' +
            '<a class="k-button" id="clearCategoryFilter">Clear</a>' +
            '</span>' +
            '</span>';
         $('#categoryTreeWindow div.k-grid-toolbar').html(toolbarTemplateString);

         treeList.bind('dataBound', function(e) {
            if ($('#Name').val() != '') {
            }
         });

         $('#assignCategories').click(function () {
            var categories;
            if ($('#CategorySelectMode').val() == "search") {
               categories = $('#' + searchCategoriesElement).data('kendoMultiSelect');
            } else {
               categories = $('#' + otherCategoriesElement).data('kendoMultiSelect');
            }

            var multi = $('#CategoryMulti').data('kendoMultiSelect');
            if (categories != undefined) {
               var values = multi.value();
               $.each(values, function(index, value) {
                  var selectedItem = multi.dataSource.get(value);
                  if (categories.dataSource.get(value) == undefined) {
                     categories.dataSource.add({ id: selectedItem.id, CategoryName: selectedItem.CategoryName });
                  }
               });
               categories.value(values);
            }
            $('#closeCategoryWindow').trigger('click');
         });

         $('#addCategory').click(function() {
            var selectedItem = treeList.dataItem(treeList.select());

            var multi = $('#CategoryMulti').data('kendoMultiSelect');
            var values = multi.value();

            if (values.indexOf(selectedItem.CategoryId) >= 0) return;
            if (multi.dataSource.get(selectedItem.CategoryId) == undefined) {
               multi.dataSource.add({ id: selectedItem.CategoryId, CategoryName: selectedItem.CategoryName });
            }

            values.push(selectedItem.CategoryId);
            multi.value(values);
            multi.trigger('change');
         });

         $('#clearCategory').click(function() {
            var multi = $('#CategoryMulti').data('kendoMultiSelect');
            multi.value([]);
            multi.trigger('change');
         });

         var win = this;
         $('#closeCategoryWindow').click(function() {
            win.close();
         });

      },
      open: function() {
         var treeList = $('#CategoryTree').data('kendoTreeList');
         treeList.dataSource.read();
      }
   });
}

function onCategoryTreeDataBound(e) {
   $('#categoryTreeWindow').data('kendoWindow').center();

   $($($('#CategoryMulti').siblings()[0]).children()[1]).on('keypress', function(event) {
      event.preventDefault();
   });
}

function onCategoryMultiOpen(e) {
   e.preventDefault();
   return false;
}

function createLedgerTreeWindow(ledgerType) {
   $('#ledgerTreeWindow').kendoWindow({
      modal: true,
      content: '/Inventory/GeneralLedger/LedgerSelector?ledgerType=' + ledgerType,
      title: 'Select general ledger',
      scrollable: false,
      visible: false,
      width: 450,
      refresh: function() {
         var treeList = $('#LedgerTree').data('kendoTreeList');
         $('#assignLedger').click(function() {
            var selectedLedger = treeList.dataItem(treeList.select());
            $('#LedgerId').val(selectedLedger.LedgerId);
            $('#GLC').val(selectedLedger.Code);
            $('#closeLedgerWindow').trigger('click');
         });

         $('#clearLedger').click(function() {
            $('#LedgerId').val('');
            $('#GLC').val('');
            $('#closeLedgerWindow').trigger('click');
         });

         var win = this;
         $('#closeLedgerWindow').click(function() {
            win.close();
         });
         treeList.bind('dataBound', function(e) {
            var ledgerId = $('#LedgerId').val();
            if (ledgerId != '') {
               var item = treeList.dataSource.get(ledgerId);
               if (item == null) return;
               var uid = item.uid;
               var row = $('#LedgerTree [data-uid=' + uid + ']');
               treeList.select(row);
            }
         });
         treeList.dataSource.read();
      },
      open: function() {
         var treeList = $('#LedgerTree').data('kendoTreeList');
         treeList.dataSource.read();
      },
   });
}

function setProductDetails(productId) {
   if (productId == null || productId == '' || productId == 0) return;
   $.ajax({
      type: 'GET',
      aysnc: false,
      url: '/Inventory/Products/GetDetails/' + productId,
      success: function(data) {
         $('#productDescription').text(data.Description);
         $('#productSPC').text(data.SPC);
         $('#productUPN').text(data.UPN);
         $('#productInformation').removeClass('hidden');
      },
      error: function(data) {
         getAlert('danger', 'Unable to find product in the system. Please check it has not been deleted.');
      }
   });
}
function applyGridTooltips() {
   $('td[role=gridcell], th a').bind('mouseenter', function () {
      var $this = $(this);

      if (this.offsetWidth < this.scrollWidth && !$this.attr('title')) {
         $this.attr('title', $this.text());
      }
   });
}

//http://stackoverflow.com/questions/18082/validate-decimal-numbers-in-javascript-isnumeric/1830844#1830844
function isNumeric(n) {
   return !isNaN(parseFloat(n)) && isFinite(n);
}