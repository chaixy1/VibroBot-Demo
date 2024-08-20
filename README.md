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

  ![Gesture Scene Screenshot](VibroBot-Demo/Figures/gesture_demo.png)

3. **Start the Demo**
   - Click the "Play" button in the Unity editor to enter the demo homepage.

4. **Select the Gesture to Guide**
   - Use the interface to choose the gesture you want to demonstrate.

5. **Begin the Gesture Demonstration**
   - After selecting the gesture, click "Begin" to start the demonstration.

6. **View Comparison Results**
   - Observe the comparison results displayed in real-time as you perform the gesture.

7. **Analyze Finger Error**
   - Check the error for each finger, which will be shown on the side of the interface, to understand which adjustments may be needed.

## Experiments and Results
Our experiments have shown that the VibroBot system significantly enhances the ability of participants with limited vision to perform standard grab actions. The system provides continuous haptic guidance until the successful grab is achieved, as defined by the criteria of 80% or more joint angles within an acceptable error range.

## Contributing
We welcome contributions to the VibroBot Gesture Guidance System. Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## License
This project is licensed under the [LICENSE NAME] - see the [LICENSE](LICENSE) file for details.

## Acknowledgments
- Special thanks to Google for the MediaPipe hand recognition algorithm.
- Acknowledgment to all contributors and testers of the VibroBot system.
