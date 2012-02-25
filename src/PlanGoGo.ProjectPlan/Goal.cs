using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanGoGo.ProjectPlan
{

    //From memory, I want
    /*
     * Man days
     * Min elasped days
     * Max Concurrent streams (ie what is the max you could parallelise)
     * Milestones ObservableList<Goal>
     
     */
    public class Goal : INotifyPropertyChanged
    {
        private int _manDays;


        public int ManDays
        {
            get { return _manDays; }
            set
            {
                if(_manDays!=value)
                {
                    _manDays = value;
                    OnPropertyChanged("ManDays");
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
