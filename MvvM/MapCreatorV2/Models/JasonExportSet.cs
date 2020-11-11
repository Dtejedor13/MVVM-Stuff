using System.Collections.Generic;

namespace MapCreatorV2.Models
{
    public class JasonExportSet
    {

        public int RowCount { get; set; }
        public int ColumnCount { get; set; }

        public List<Export> MetaData { get; set; }
        public JasonExportSet(int rowCount, int columnCount, List<Export> metaData)
        {
            this.RowCount = rowCount;
            this.ColumnCount = columnCount;
            this.MetaData = metaData;
        }
    }
}
