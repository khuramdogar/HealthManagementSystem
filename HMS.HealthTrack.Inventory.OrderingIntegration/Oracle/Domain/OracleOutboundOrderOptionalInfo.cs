using System;

namespace HMS.HealthTrack.Inventory.OrderingIntegration.Oracle
{
    internal struct OracleOutboundOrderOptionalInfo
    {
        private readonly DateTime? _fmisDeliveryDate;
        private readonly string _informationTemplateAttribute1;
        private readonly string _informationTemplateAttribute2;
        private readonly string _informationTemplateAttribute3;
        private readonly string _informationTemplateAttribute4;
        private readonly string _informationTemplateAttribute5;
        private readonly string _informationTemplateAttribute6;
        private readonly string _informationTemplateAttribute7;
        private readonly string _informationTemplateAttribute8;
        private readonly string _requistionNumberFlag;
        private readonly string _vendorNote;

        public OracleOutboundOrderOptionalInfo(DateTime? fmisDeliveryDate, string informationTemplateAttribute1, 
            string informationTemplateAttribute2, string informationTemplateAttribute3, string informationTemplateAttribute4, 
            string informationTemplateAttribute5, string informationTemplateAttribute6, string informationTemplateAttribute7, 
            string informationTemplateAttribute8, string requistionNumberFlag, string vendorNote) : this()
        {
            _fmisDeliveryDate = fmisDeliveryDate;
            _informationTemplateAttribute1 = informationTemplateAttribute1;
            _informationTemplateAttribute2 = informationTemplateAttribute2;
            _informationTemplateAttribute3 = informationTemplateAttribute3;
            _informationTemplateAttribute4 = informationTemplateAttribute4;
            _informationTemplateAttribute5 = informationTemplateAttribute5;
            _informationTemplateAttribute6 = informationTemplateAttribute6;
            _informationTemplateAttribute7 = informationTemplateAttribute7;
            _informationTemplateAttribute8 = informationTemplateAttribute8;
            _requistionNumberFlag = requistionNumberFlag;
            _vendorNote = vendorNote;
        }

        public DateTime? FmisDeliveryDate
        {
            get { return _fmisDeliveryDate; }
        }

        public string InformationTemplateAttribute1
        {
            get { return _informationTemplateAttribute1; }
        }

        public string InformationTemplateAttribute2
        {
            get { return _informationTemplateAttribute2; }
        }

        public string InformationTemplateAttribute3
        {
            get { return _informationTemplateAttribute3; }
        }

        public string InformationTemplateAttribute4
        {
            get { return _informationTemplateAttribute4; }
        }

        public string InformationTemplateAttribute5
        {
            get { return _informationTemplateAttribute5; }
        }

        public string InformationTemplateAttribute6
        {
            get { return _informationTemplateAttribute6; }
        }

        public string InformationTemplateAttribute7
        {
            get { return _informationTemplateAttribute7; }
        }

        public string InformationTemplateAttribute8
        {
            get { return _informationTemplateAttribute8; }
        }

        public string RequistionNumberFlag
        {
            get { return _requistionNumberFlag; }
        }

        public string VendorNote
        {
            get { return _vendorNote; }
        }
    }
}
