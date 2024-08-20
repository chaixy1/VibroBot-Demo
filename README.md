# VibroBot Demo

## Overview
The VibroBot Gesture Guidance System is an innovative real-time gesture guidance system designed to assist users, particularly those with limited vision, in performing precise hand gestures for virtual scenarios. By utilizing five VibroBots worn on each finger, the system provides haptic feedback to guide users through the process of learning and perfecting grab actions.

## Key Features
- **Real-time Gesture Recognition**: Captures and processes hand gestures at 30 frames per second.
- **Wireless Connectivity**: Interconnects components via a wireless local area network.
- **MediaPipe Integration**: Leverages Google’s open-source hand recognition for accurate joint positioning.
- **Haptic Feedback**: VibroBots provide real-time guidance through vibration modes.
- **Color-coded Error Representation**: Visual feedback indicates areas needing improvement.

## System Components
- **Camera**: Captures the user's hand gestures.
- **Virtual Scene**: Displays real-time gesture data and error analysis.
- **VibroBots**: Wearable devices that vibrate to guide finger positioning.
- **MediaPipe Algorithm**: Calculates hand joint positions from captured images.

## Installation
To install and set up the VibroBot Gesture Guidance System, follow these steps carefully. Ensure you have Python and Unity installed on your system before proceeding.
### Python Setup

1. **Clone the Repository**
- Open your terminal or command prompt and run:
   ```bash
   git clone https://github.com/chaixy1/VibroBot-Demo.git
2. **Install Dependencies**
- Install Python 3.7 or later.
- Install the following Python packages:
  ```
  pip install mediapipe
  ```

### Unity Setup

1. **Unity Version**
- Ensure you have Unity version 2021.3.24 or later installed.

2. **Import the Unity Project**
- Open Unity and create a new project.
- In the Unity Editor, go to `Assets` > `Import Package` > `Custom Package`.
- Navigate to the cloned repository and select the Unity project file to import.

3. **Configure the System**
- In the Unity Editor, open the `Gesture` or `Grab` scene.
- Ensure the camera component is properly configured to capture the necessary input.
- Adjust any network or other system settings as required.


## Usage
Follow these steps to use the VibroBot Gesture Guidance System effectively.
### Start the System
1. **Run the Main Application**
   - Navigate to the `VibroBot-Demo/Python` directory.
   - Execute the main Python script to begin gesture capture and recognition:
     ```bash
     python mediapipe_python.py
     ```
   - Ensure that the script is configured with the correct network settings for your environment.

2. **Configure Network Settings**
   - By default, the script uses the following network settings:
     ```python
     udp_ip = "127.0.0.1"
     udp_port = 12333
     ```
   - Modify the `udp_ip` and `udp_port` variables in the script to match your network's IP address and desired port.

### Gesture Demo

To perform a gesture demonstration using the Unity application, follow these steps:

1. **Open Unity Hub and Import the Gesture Project**
   - Launch Unity Hub and add the Gesture project by navigating to the directory where you have cloned or downloaded the VibroBot Gesture Guidance System.

2. **Open the Gesture Scene**
   - Within the Unity project, go to `Assets/Scenes` and open the `Gesture.unity` file.
   - Here is a screenshot of what you should see:
   
   ![Image text](https://github.com/chaixy1/VibroBot-Demo/blob/main/Figures/gesture_demo.png)

4. **Start the Demo**
   - Click the "Play" button in the Unity editor to enter the demo homepage.

5. **Select the Gesture to Guide**
   - Use the interface to choose the gesture you want to demonstrate.

   ![Image text](https://github.com/chaixy1/VibroBot-Demo/blob/main/Figures/gesture_demo2.png)

6. **Begin the Gesture Demonstration**
   - After selecting the gesture, click "Begin" to start the demonstration.

   ![Image text](https://github.com/chaixy1/VibroBot-Demo/blob/main/Figures/gesture_demo3.png)

7. **View Comparison Results**
   - Observe the comparison results displayed in real-time as you perform the gesture.

   ![Image text](https://github.com/chaixy1/VibroBot-Demo/blob/main/Figures/gesture_demo4.png)

8. **Analyze Finger Error**
   - Check the error for each finger, which will be shown on the side of the interface, to understand which adjustments may be needed.

### Grab Demo

Follow these steps to perform the Grab demo using the Unity application:

1. **Import the Grab Scene in Unity Hub**
   - Launch Unity Hub and open the project containing the Grab demo.
   - Navigate to the `Assets/Scenes` directory and select the `Grab.unity` file.

2. **Open and Run the Grab Scene**
   - Double-click on `Grab.unity` to open the scene in the Unity editor.
   - Press the "Play" button to start the Grab demo.

3. **Perform the Grasping Gesture**
   - Position your hand in front of the camera and perform the grasping gesture.

4. **Evaluate the Gesture**
   - After performing the gesture, click the `checkbutton` to receive feedback on your grasp.

5. **Interpret the Results**
   - The system will indicate one of three states based on your gesture:
     - **Ungrab**: The gesture was not recognized as a grasp.
     - **Grab**: The gesture was successfully recognized as a grasp.
     - **Overgrab**: The gesture was too forceful or excessive.

   ![Image text](https://github.com/chaixy1/VibroBot-Demo/blob/main/Figures/grab_demo.png)

6. **Restart the Demo**
   - To perform the demo again, click the `restart` button to reset the scene and begin a new attempt.

## Video Tutorial

For a step-by-step video guide on setting up and using VibroBot, please watch the tutorial below:

[Watch the setup and usage tutorial on YouTube](https://www.youtube.com/watch?v=6eU5iwbdGzk&t=22s)

## License
This project is licensed under the MIT - see the [LICENSE](LICENSE) file for details.

## Acknowledgments
- Special thanks to Google for the MediaPipe hand recognition algorithm.
- Acknowledgment to all contributors and testers of the VibroBot system.
