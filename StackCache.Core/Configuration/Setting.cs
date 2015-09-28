namespace StackCache.Core.Configuration
{
    using System;

    /// <summary>
    /// Base setting class
    /// </summary>
    public abstract class Setting
    {
        /// <summary>
        /// Notifies config file change on disk
        /// </summary>
        public event EventHandler SettingFileChanged;

        public virtual void OnSettingFileChanged()
        {
            EventHandler ev = this.SettingFileChanged;
            ev?.Invoke(this,new EventArgs());
        }
    }
}