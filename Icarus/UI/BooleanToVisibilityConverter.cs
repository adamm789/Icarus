using System.Windows;

namespace Icarus.UI
{
    /*
     * Usage
     *     <Grid PreviewMouseWheel="ListView_PreviewMouseWheel">
        <Grid.Resources>
            <ui:BooleanToVisibilityConverter 
                x:Key="BooleanToVisibilityConverter"
                True="Collapsed"
                False="Visible"/>
        </Grid.Resources>
    </Grid>
     */

    public sealed class BooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public BooleanToVisibilityConverter() :
    base(Visibility.Visible, Visibility.Collapsed)
        { }
    }
}
