using System;
using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
    public class CrmLegalsViewModel : IndexViewModel
    {
        public IEnumerable<CrmLegal> Items { get; set; }
    }
}