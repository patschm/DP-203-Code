﻿using AppInsightData;
using AppInsightIngest.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AppInsightIngest.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly WebRequestSimulator _requester;
        public MainViewModel(WebRequestSimulator wrsim)
        {
            _requester = wrsim;
        }

        private int[] _productIDs = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144 };
        public int[] ProductIDs
        {
            get { return _productIDs; }
            set { 
                _productIDs = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProductIDs)));
            }
        }

        private int[] productTypeIDs = { 1000, 1001, 1002, 1003, 1004, 1005, 1006, 1007, 1008, 1009, 1010, 1011, 1012, 1013, 1014, 1015, 1016, 1017, 1018, 1019, 1020 };
        public int[] ProductTypeIDs
        {
            get { return productTypeIDs; }
            set { 
                productTypeIDs = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProductTypeIDs)));
            }
        }

        private int _nrProductRequests = 0;
        public int NrProductRequests
        {
            get { return _nrProductRequests; }
            set { 
                _nrProductRequests = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NrProductRequests)));
            }
        }
        private int _nrProductTypeRequests = 0;
        public int NrProductTypeRequests
        {
            get { return _nrProductTypeRequests; }
            set
            {
                _nrProductTypeRequests = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NrProductTypeRequests)));
            }
        }
        private bool _isProductRequestVisible = true;
        public bool IsProductRequestVisible
        {
            get { return _isProductRequestVisible; }
            set { _isProductRequestVisible = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsProductRequestVisible)));
            }
        }
        private bool _isProductTypeRequestVisible = true;
        public bool IsProductTypeRequestVisible
        {
            get { return _isProductTypeRequestVisible; }
            set
            {
                _isProductTypeRequestVisible = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsProductTypeRequestVisible)));
            }
        }

        private CancellationTokenSource _productCancel = null;
        private ICommand _cancelProductRequests;
        public ICommand CancelProductRequests
        {
            get
            {
                return _cancelProductRequests ??= new CommandHandler(() =>
                {
                    _productCancel.Cancel();
                    _requester.ProductRequestSent -= (o, e) => NrProductRequests++;
                    IsProductRequestVisible = true;
                }, () => true);
            }
        }
        private ICommand _productRequests;       
        public ICommand StartProductRequests
        {
            get
            {            
                return _productRequests ??= new CommandHandler(async () =>
                {
                    _productCancel = new CancellationTokenSource();
                    IsProductRequestVisible = false;
                    NrProductRequests = 0;
                    _requester.ProductRequestSent += (o, e) => NrProductRequests++;
                    await _requester.StartGeneratingProductUrls(ProductIDs, token: _productCancel.Token);
                }, () => true);
            }
        }

        private CancellationTokenSource _productTypeCancel = null;
        private ICommand _cancelProductTypeRequests;
        public ICommand CancelProductTypeRequests
        {
            get
            {
                return _cancelProductTypeRequests ??= new CommandHandler(() =>
                {
                    _productTypeCancel.Cancel();
                    _requester.ProductTypeRequestSent -= (o, e) => NrProductTypeRequests++;
                    IsProductTypeRequestVisible = true;
                }, () => true);
            }
        }
        private ICommand _productTypeRequests;
        public ICommand StartProductTypeRequests
        {
            get
            {
               
                return _productTypeRequests ??= new CommandHandler(async () =>
                {
                    _productTypeCancel = new CancellationTokenSource();
                    NrProductTypeRequests = 0;
                    IsProductTypeRequestVisible = false;
                    _requester.ProductTypeRequestSent += (o, e) => NrProductTypeRequests++;
                    await _requester.StartGeneratingProductTypeUrls(ProductTypeIDs,  token: _productTypeCancel.Token);
                }, () => true);
            }
        }
    }
}
