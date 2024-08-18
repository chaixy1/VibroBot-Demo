import mediapipe as mp
import cv2
import socket
import numpy as np
import math

# UDP 设置
udp_ip = "127.0.0.1"
udp_port = 12333
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# MediaPipe 手部模型初始化
mp_hands = mp.solutions.hands
hands = mp_hands.Hands()
mp_drawing = mp.solutions.drawing_utils




# 计算空间向量夹角
def calculate_angle(point_a, point_b, point_c):
    # 创建向量 BA 和 BC
    ba = [point_a[i] - point_b[i] for i in range(3)]
    bc = [point_c[i] - point_b[i] for i in range(3)]

    # 计算点积
    dot_product = sum(ba[i]*bc[i] for i in range(3))

    # 计算向量的模
    magnitude_ba = math.sqrt(sum(ba[i]**2 for i in range(3)))
    magnitude_bc = math.sqrt(sum(bc[i]**2 for i in range(3)))

    # 计算角度（弧度）
    angle = math.acos(dot_product / (magnitude_ba * magnitude_bc))

    # 转换为度
    angle_deg = angle * 180 / math.pi

    return angle_deg

# 用于从 MediaPipe 数据中获取指定关键点坐标的函数
def get_landmark_coord(landmarks, idx):
    return np.array([landmarks[idx].x, landmarks[idx].y, landmarks[idx].z])


# 打开摄像头
cap = cv2.VideoCapture(0)

fps = cap.get(cv2.CAP_PROP_FPS)
print(f"摄像头帧率: {fps}")

while cap.isOpened():
    success, image = cap.read()
    if not success:
        continue

    # 处理图像
    image = cv2.cvtColor(cv2.flip(image, 1), cv2.COLOR_BGR2RGB)
    results = hands.process(image)

    # 绘制手部关键点
    image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
    if results.multi_hand_landmarks:
        for hand_landmarks in results.multi_hand_landmarks:
            mp_drawing.draw_landmarks(image, hand_landmarks, mp_hands.HAND_CONNECTIONS)

            # 计算欧拉角
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

            # 发送数据
            # 将数据转换为以逗号为间隔的字符串，保留六位小数
            data_string = ','.join(['{:.6f}'.format(row) for row in euler_angles])
            message = data_string
            print(message)
            sock.sendto(message.encode(), (udp_ip, udp_port))

    # 显示图像
    cv2.imshow('MediaPipe Hands', image)
    if cv2.waitKey(5) & 0xFF == 27:
        break

cap.release()
cv2.destroyAllWindows()
sock.close()
