
using System;
public interface IAdsFreeProvider
{
    public void Init();
    public string GetPrice(string name);
    public void Buy(string name, Action onSuccess); 
    }
