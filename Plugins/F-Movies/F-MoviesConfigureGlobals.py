# This is a dummy file needed for the XAML designer to work properly and is not used
# at run time. It should contain only the "__init__" routine. Any event handlers placed 
# here by the XAML designer should be moved to the main plugin file.
class FMoviesStreamingConfigureGlobals(Window):
    def __init__(self):
        wpf.LoadComponent(self, 'F-MovieConfigureGlobals.xaml')
