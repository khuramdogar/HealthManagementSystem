using System;
using System.Text;

namespace HMS.HealthTrack.Web.Areas.Inventory.Reporting
{
	public static class Formatter
	{
		public static string FormattedAddress(string nameLine, string line1, string line2, string suburb, string state, string postcode, string phone, string fax, bool useNewlines)
		{
			return FormattedAddress(nameLine, null, line1, line2, null, null, null, suburb, state, postcode, phone, fax, useNewlines);
		}

		public static string FormattedAddress(string nameLine, string line1, string line2, string line3, string line4, string line5, string suburb, string state, string postcode, string phone, string fax, bool useNewlines)
		{
			return FormattedAddress(nameLine, null, line1, line2, line3, line4, line5, suburb, state, postcode, phone, fax, useNewlines);
		}

		public static string FormattedAddress(string nameLine, string departmentLine, string line1, string line2, string line3, string line4, string line5, string suburb, string state, string postcode, string phone, string fax, bool useNewlines)
		{
			string seperator = useNewlines ? Environment.NewLine : ", ";

			StringBuilder sb = new StringBuilder(300);
			sb.Append(FormattedAddress(nameLine, null, departmentLine, line1, line2, line3, suburb, state, postcode, useNewlines));
			if (phone != null && phone.Trim().Length > 0)
			{
				sb.Append(seperator);
				sb.Append("Tel: ");
				sb.Append(phone);
			}
			if (fax != null && fax.Trim().Length > 0)
			{
				sb.Append(seperator);
				sb.Append("Fax: ");
				sb.Append(fax);
			}

			return sb.ToString();
		}

		public static string FormattedAddress(string nameLine, string companyName, string companyDeptName, string line1, string line2, string line3, string suburb, string state, string postcode, bool useNewlines)
		{
			StringBuilder sb = new StringBuilder(300);
			string seperator = useNewlines ? Environment.NewLine : ", ";

			if (nameLine == null) nameLine = String.Empty;
			else nameLine = nameLine.Trim();
			if (companyName == null) companyName = String.Empty;
			else companyName = companyName.Trim();
			if (companyDeptName == null) companyDeptName = String.Empty;
			else companyDeptName = companyDeptName.Trim();
			if (line1 == null) line1 = String.Empty;
			else line1 = line1.Trim();
			if (line2 == null) line2 = String.Empty;
			else line2 = line2.Trim();
			if (line3 == null) line3 = String.Empty;
			else line3 = line3.Trim();
			if (suburb == null) suburb = String.Empty;
			else suburb = suburb.Trim();
			if (postcode == null) postcode = String.Empty;
			else postcode = postcode.Trim();
			if (state == null) state = String.Empty;
			else state = state.Trim();


			if (nameLine.Length > 0)
			{
				sb.Append(nameLine);
				sb.Append(seperator);
			}
			if (companyName.Length > 0)
			{
				sb.Append(companyName);
				sb.Append(seperator);
			}
			if (companyDeptName.Length > 0)
			{
				sb.Append(companyDeptName);
				sb.Append(seperator);
			}
			if (line1.Length > 0)
			{
				sb.Append(line1);
				sb.Append(seperator);
			}
			if (line2.Length > 0)
			{
				sb.Append(line2);
				sb.Append(seperator);
			}
			if (line3.Length > 0)
			{
				sb.Append(line3);
				sb.Append(seperator);
			}
			if (suburb.Length > 0) sb.Append(suburb);

			if (suburb.Length > 0 && state.Length > 0) sb.Append(" ");
			if (state.Length > 0) sb.Append(state);

			if ((suburb.Length > 0 || state.Length > 0) && postcode.Length > 0) sb.Append("  ");
			if (postcode.Length > 0) sb.Append(postcode);

			return sb.ToString();
		}
	}
}