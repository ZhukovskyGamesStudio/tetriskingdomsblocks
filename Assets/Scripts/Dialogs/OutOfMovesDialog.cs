using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class OutOfMovesDialog : DialogBase {
    [SerializeField]
    private TextMeshProUGUI _balanceText, _costText;
    
    private Action _clickAdd, _clickClose;

    public override void SetData(object data) {
        Data dialogData = data as Data;

        _clickAdd = dialogData.ClickAdd;
        _clickClose = dialogData.ClickClose;
        _balanceText.text = dialogData.Balance.ToString();
        _costText.text = _costText.text.Replace("{cost}", dialogData.Cost.ToString());
    }

    public void ClickAdd() {
        _clickAdd.Invoke();
        Hide().Forget();
    }

    public void ClickClose() {
        _clickClose.Invoke();
        Hide().Forget();
    }

    public void ClickBalance() {
        Hide().Forget();
        
        var dialog = new DialogWithData {
            DialogType = typeof(RealShopDialog),
            Data = new RealShopDialog.Data {
                Balance = 1000,
                OffersGroups = new[] {
                    new OffersGroupData {
                        Title = "Title",
                        Offers = new[] {
                            new OfferData {
                                Price = 1000,
                                Title = "Title",
                                Resources = new[] {
                                    new Tuple<Sprite, string>(null, "42")
                                }
                            }
                        }
                    }
                }
            }
        };
        DialogsManager.Instance.ShowDialogWithData(dialog);
    }

    [Serializable]
    public class Data {
        public Action ClickAdd;
        public Action ClickClose;
        public int Balance;
        public int Cost;
    }
}