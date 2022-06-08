using System.Collections.Generic;

namespace VTS_XYPlugin_Common
{
    public class DropItemDataBase
    {
        public List<DropItemData> DropItems = new List<DropItemData>();

        public bool HasItem(string name)
        {
            for (int i = 0; i < DropItems.Count; i++)
            {
                if (DropItems[i].Name == name)
                {
                    return true;
                }
            }
            return false;
        }
    }
}