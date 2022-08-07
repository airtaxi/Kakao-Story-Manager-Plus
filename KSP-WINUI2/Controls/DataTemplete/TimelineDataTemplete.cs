using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static StoryApi.ApiHandler.DataType.CommentData;

namespace KSP_WINUI2.Controls.DataTemplete
{
    internal class TimelineDataTemplete : INotifyPropertyChanged
    {
        private string displayName;
        public string DisplayName
        {
            get => displayName; set
            {
                displayName = value;
                NotifyPropertyChanged();
            }
        }

        private string content;
        public string Content
        {
            get => content; set
            {
                content = value;
                NotifyPropertyChanged();
            }
        }

        public TimelineDataTemplete(PostData post, bool isShare = false, bool isOverlay = false)
        {
            DisplayName = post.actor.display_name;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
