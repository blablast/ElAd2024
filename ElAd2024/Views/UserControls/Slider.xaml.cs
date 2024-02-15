using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ElAd2024.Views.UserControls;

public sealed partial class Slider : UserControl
{
    public Slider()
    {
        InitializeComponent();
    }

    //DependencyProperties

    public double TickFrequency
    {
        get => (double)GetValue(TickFrequencyProperty);
        set => SetValue(TickFrequencyProperty, value);
    }

    public double SelectionStart
    {
        get => (double)GetValue(SelectionStartProperty);
        set => SetValue(SelectionStartProperty, value);
    }

    public double SelectionEnd
    {
        get => (double)GetValue(SelectionEndProperty);
        set => SetValue(SelectionEndProperty, value);
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public int SliderWidth
    {
        get => (int)GetValue(SliderWidthProperty);
        set => SetValue(SliderWidthProperty, value);
    }

    public bool MaxToMin
    {
        get => (bool)GetValue(MaxToMinProperty);
        set => SetValue(MaxToMinProperty, value);
    }


    // Using a DependencyProperty as the backing store...

    public static readonly DependencyProperty TickFrequencyProperty =
        DependencyProperty.Register("TickFrequency", typeof(double), typeof(Slider), new PropertyMetadata(0));

    public static readonly DependencyProperty SelectionStartProperty =
        DependencyProperty.Register("SelectionStart", typeof(double), typeof(Slider), new PropertyMetadata(0));

    public static readonly DependencyProperty SelectionEndProperty =
        DependencyProperty.Register("SelectionEnd", typeof(double), typeof(Slider), new PropertyMetadata(0));

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register("Minimum", typeof(double), typeof(Slider), new PropertyMetadata(0));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register("Maximum", typeof(double), typeof(Slider), new PropertyMetadata(0));

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register("Header", typeof(string), typeof(Slider), new PropertyMetadata(0));

    public static readonly DependencyProperty SliderWidthProperty =
        DependencyProperty.Register("SliderWidth", typeof(int), typeof(Slider), new PropertyMetadata(0));

    public static readonly DependencyProperty MaxToMinProperty =
        DependencyProperty.Register("MinToMax", typeof(bool), typeof(Slider), new PropertyMetadata(0));

}
