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
- **Start the System**: Run the main application to begin gesture capture and recognition.
- **Wear VibroBots**: Equip the VibroBots on each finger as instructed.
- **Perform Gestures**: Follow the virtual scene's feedback to adjust and perfect your gestures.

## Experiments and Results
Our experiments have shown that the VibroBot system significantly enhances the ability of participants with limited vision to perform standard grab actions. The system provides continuous haptic guidance until the successful grab is achieved, as defined by the criteria of 80% or more joint angles within an acceptable error range.

## Contributing
We welcome contributions to the VibroBot Gesture Guidance System. Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## License
This project is licensed under the [LICENSE NAME] - see the [LICENSE](LICENSE) file for details.

## Acknowledgments
- Special thanks to Google for the MediaPipe hand recognition algorithm.
- Acknowledgment to all contributors and testers of the VibroBot system.
