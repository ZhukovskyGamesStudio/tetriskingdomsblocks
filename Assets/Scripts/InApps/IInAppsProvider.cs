using System;

    public interface IInAppsProvider {

        public void Init();
        public string GetPrice(InApsTypes name);
        public void Buy(InApsTypes name, Action onSuccess);
    }
