using System;

namespace JSON_DAL
{
    public class AssetAttribute : Attribute
    {
        private readonly string folder;

        public AssetAttribute(string folder)
        {
            this.folder = folder;
        }
        public string Folder() => folder;
    }
}