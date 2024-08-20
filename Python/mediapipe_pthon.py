import mediapipe as mp
import cv2
import socket
import numpy as np
import math

# UDP setup
# This IP address is where the recognized finger bend angles are sent.
# You can modify this to another location within the local network, 
# but you also need to set up the receiver in Unity to match.
udp_ip = "127.0.0.1"
udp_port = 12333
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# Initialize MediaPipe Hands model
mp_hands = mp.solutions.hands
hands = mp_hands.Hands()
mp_drawing = mp.solutions.drawing_utils

# Function to calculate the angle between vectors in 3D space
def calculate_angle(point_a, point_b, point_c):
    # Create vectors BA and BC
    ba = [point_a[i] - point_b[i] for i in range(3)]
    bc = [point_c[i] - point_b[i] for i in range(3)]

    # Calculate the dot product
    dot_product = sum(ba[i]*bc[i] for i in range(3))

    # Calculate the magnitude of the vectors
    magnitude_ba = math.sqrt(sum(ba[i]**2 for i in range(3)))
    magnitude_bc = math.sqrt(sum(bc[i]**2 for i in range(3)))

    # Calculate the angle in radians
    angle = math.acos(dot_product / (magnitude_ba * magnitude_bc))

    # Convert to degrees
    angle_deg = angle * 180 / math.pi

    return angle_deg

# Function to get the coordinates of a specific landmark from MediaPipe data
def get_landmark_coord(landmarks, idx):
    return np.array([landmarks[idx].x, landmarks[idx].y, landmarks[idx].z])

# Open the webcam
cap = cv2.VideoCapture(0)

fps = cap.get(cv2.CAP_PROP_FPS)
print(f"Camera frame rate: {fps}")

while cap.isOpened():
    success, image = cap.read()
    if not success:
        continue

    # Process the image
    image = cv2.cvtColor(cv2.flip(image, 1), cv2.COLOR_BGR2RGB)
    results = hands.process(image)

    # Draw hand landmarks
    image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
    if results.multi_hand_landmarks:
        for hand_landmarks in results.multi_hand_landmarks:
            mp_drawing.draw_landmarks(image, hand_landmarks, mp_hands.HAND_CONNECTIONS)

            # Calculate Euler angles
            euler_angles = []
            for i in range(5):
                for j in range(3):
                    if j != 0:
                        point_a = get_landmark_coord(hand_landmarks.landmark, mp_hands.HandLandmark(i * 4 + j).value)
                        point_b = get_landmark_coord(hand_landmarks.landmark, mp_hands.HandLandmark(i * 4 + j + 1).value)
                        point_c = get_landmark_coord(hand_landmarks.landmark, mp_hands.HandLandmark(i * 4 + j + 2).value)
                    else:
                        point_a = get_landmark_coord(hand_landmarks.landmark, mp_hands.HandLandmark(0).value)
                        point_b = get_landmark_coord(hand_landmarks.landmark, mp_hands.HandLandmark(i * 4 + j + 1).value)
                        point_c = get_landmark_coord(hand_landmarks.landmark, mp_hands.HandLandmark(i * 4 + j + 2).value)

                    angle = calculate_angle(point_a, point_b, point_c) - 180
                    euler_angles.append(angle)

            # Send data
            # Convert data to a comma-separated string with six decimal places
            data_string = ','.join(['{:.6f}'.format(row) for row in euler_angles])
            message = data_string
            print(message)
            sock.sendto(message.encode(), (udp_ip, udp_port))

    # Display the image
    cv2.imshow('MediaPipe Hands', image)
    if cv2.waitKey(5) & 0xFF == 27:
        break

cap.release()
cv2.destroyAllWindows()
sock.close()
