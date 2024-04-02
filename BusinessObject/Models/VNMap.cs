using BusinessObject.VNProvince;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
	public class VNMap
	{
		public IList<Province>? provinces { get; set; }
		public IList<District>? districts { get; set; }	
		public IList<Ward>? wards { get; set; }	


	}
}
