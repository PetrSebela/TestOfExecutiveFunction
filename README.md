# TestOfExecutiveFunction
## How to run
No all dependencies are already included in the build folder.
The test is executed by running the `TestOfExecutiveFunction.exe` binary.

### How to use
You can specify the test size ( count of the targets the subject needs to click ) by dragging the `Test size` slider.
You can toggle aditional modifiers to make the test harder.
  Hidden variant will hide the target labels when the test begins.
  Alpha variant will require the user to click the sequence mixed with alphabet (1 - A - 2 - B - 3 - C and so on...)

On the right side your past results are displayed.

---

## How to modify
The app is build on top of the Unity game engine and uses many of its systems so it is recomended to learn the basics before modifying the code. (https://www.youtube.com/@Brackeys/featured)
To modify the app you are required to use Unity editor of version 6000.0.23f1 higher version can also work but the compatibility is not guaranteed.
Opening the project in editor will automatically install all dependencies.

All scripts are located in the Assets/Scripts folder.
The UI documents are located in the Assets/UI folder.
The main scene file is located in the Scenes/TrailMakingTest.unity

The scripts itself are documented using doxygen but the following section provides overview of existing scripts.

### Code base overview
#### TestManager
This class is the main entry point of the program and contains all the UI logic and input processing. Inputs are passed into the instance of `TrailMakingTest` class. 
Upon app startup all past results are loaded using the serialized and displayed onto the main screen.

#### Evaluator
Evaluator class provides functionality for evaluating data stored in the `TrailMakingTest` class 

#### TrailMakingTest
This class is utilized to store all use inputs before evaluation and to set up the test itself. Upon pressing the start button the `GenerateTargets` method is called. This class also provides the `OnTestBegin` callback. 

#### Sample
Sample is a class for storing the samples of user input (position, click state)

#### Target
Class representing the clickable target. It contains colision detection and functionality to place each target.

#### TestResultVisualization
Unity SGO used for data binding between test results and UI.

#### GraphElement
Custom implementation of UI element providing graph drawing functionality.

#### Serializer
Class used to saving and loding the test results to and from persistem memory. 
All results are stored in the application folder.
