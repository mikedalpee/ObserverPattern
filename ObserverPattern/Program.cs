using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObserverPattern
{
    public struct SubjectState
    {
        public int Attribute
        {
            private set;
            get;
        }
        public SubjectState(
            int attribute)
        {
            Attribute = attribute;
        }
    }
    public class ConcreteSubject : IObservable<SubjectState>
    {
        private static int _subjectNumber = 0;
        private List<IObserver<SubjectState>> _observers;
        private Dictionary<IObserver<SubjectState>, IDisposable> _unsubscribers;
        private SubjectState _state;

        internal class Unsubscriber<SubjectState> : IDisposable
        {
            private IObservable<SubjectState> _subject;
            private List<IObserver<SubjectState>> _observers;
            private IObserver<SubjectState> _observer;

            internal Unsubscriber(
                IObservable<SubjectState> subject, 
                List<IObserver<SubjectState>> observers, 
                IObserver<SubjectState> observer)
            {
                this._subject = subject;
                this._observers = observers;
                this._observer = observer;
            }

            public void Dispose()
            {
                ((ConcreteSubject)_subject).Detach((ConcreteObserver)_observer);
            }
        }
        private void Notify(
            IObserver<SubjectState> specificObserver = null)
        {
            if (_observers.Count == 0)
            {
                Console.WriteLine(
                    SubjectName+" has no Observers");
            }
            foreach (var observer in _observers)
            {
                if (specificObserver == null | observer == specificObserver)
                {
                    Console.WriteLine(
                        SubjectName + " is notifying " + 
                        ((ConcreteObserver)observer).ObserverName + 
                        " about SubjectState change");

                    observer.OnNext(_state);
                }
            }
        }     
        public SubjectState State
        {
            get
            {
                return _state;
            }
            set
            {
                if (!value.Equals(_state))
                {
                    _state = value;

                    Console.WriteLine(
                        SubjectName+"'s SubjectState changed to: "+
                        State.Attribute);

                    Notify();
                }
            }
        }
        public int SubjectNumber
        {
            private set;
            get;
        }
        public String SubjectName
        {
            get
            {
                return "Subject " + SubjectNumber;
            }
        }
        public ConcreteSubject(
            SubjectState initialState)
        {
            _observers = new List<IObserver<SubjectState>>();
            _unsubscribers = new Dictionary<IObserver<SubjectState>, IDisposable>();
            _subjectNumber++;
            SubjectNumber = _subjectNumber;
            State = initialState;
        }
        private void Attach(
            IObserver<SubjectState> observer)
        {
            if (!_observers.Contains(observer))
            {
                Console.WriteLine(
                    ((ConcreteObserver)observer).ObserverName + 
                    " is attached to " + SubjectName);

                _observers.Add(observer);

                // Provide observer with existing data.

                Notify(observer);
            }
        }
        private void Detach(
            IObserver<SubjectState> observer)
        {
            if (_observers.Contains(observer))
            {
                Console.WriteLine(
                    ((ConcreteObserver)observer).ObserverName + 
                    " is detached from " + SubjectName);

                _observers.Remove(observer);
            }

            if (_unsubscribers.ContainsKey(observer))
            {
                _unsubscribers.Remove(observer);
            }
        }
        public IDisposable Subscribe(
            IObserver<SubjectState> observer)
        {
            Attach(observer);

            IDisposable unsubscriber;

            if (_unsubscribers.ContainsKey(observer))
            {
                unsubscriber = _unsubscribers[observer];
            }
            else
            {
                unsubscriber = new Unsubscriber<SubjectState>(
                                    this, _observers, observer);

                _unsubscribers[observer] = unsubscriber;
            }

            return unsubscriber;
        }
    }
    class ConcreteObserver : IObserver<SubjectState>
    {
        private static int _observerNumber = 0;

        public int ObserverNumber
        {
            private set;
            get;
        }
        public String ObserverName
        {
            get
            {
                return "Observer " + ObserverNumber;
            }
        }
        public ConcreteObserver()
        {
            _observerNumber++;

            ObserverNumber = _observerNumber;
        }
        public void OnCompleted()
        {
            throw new NotImplementedException();
        }
        public void OnError(
            Exception error)
        {
            throw new NotImplementedException();
        }
        public void OnNext(
            SubjectState value)
        {
            Console.WriteLine(
                ObserverName + " received new SubjectState: "+ 
                value.Attribute);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(
                "** Creating Subjects **");

            ConcreteSubject subject1 = new ConcreteSubject(new SubjectState(1));
            ConcreteSubject subject2 = new ConcreteSubject(new SubjectState(2));

            Console.WriteLine(
                "** Creating Observers **");

            ConcreteObserver observer1 = new ConcreteObserver();
            ConcreteObserver observer2 = new ConcreteObserver();
            ConcreteObserver observer3 = new ConcreteObserver();
            ConcreteObserver observer4 = new ConcreteObserver();

            Console.WriteLine(
                "** Attaching Observers to Subjects **");

            IDisposable observer1_subject1 = subject1.Subscribe(observer1);
            IDisposable observer2_subject1 = subject1.Subscribe(observer2);
            IDisposable observer1_subject2 = subject2.Subscribe(observer1);
            IDisposable observer3_subject2 = subject2.Subscribe(observer3);
            IDisposable observer4_subject2 = subject2.Subscribe(observer4);

            Console.WriteLine(
                "** Modifying subject states **");

            subject1.State = new SubjectState(10);
            subject2.State = new SubjectState(20);

            Console.WriteLine(
                "** Detaching Observer 1 from Subject 1 and Subject 2 **");

            observer1_subject1.Dispose();
            observer1_subject2.Dispose();

            Console.WriteLine(
                "** Modifying subject states **");

            subject1.State = new SubjectState(88);
            subject2.State = new SubjectState(99);
        }
    }
}
