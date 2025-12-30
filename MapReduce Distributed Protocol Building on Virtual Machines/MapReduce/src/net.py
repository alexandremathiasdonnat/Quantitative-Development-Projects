import socket, struct, pickle, time

def send_obj(sock, obj):
    data = pickle.dumps(obj, protocol=pickle.HIGHEST_PROTOCOL)
    sock.sendall(struct.pack("!I", len(data)) + data)

def recv_obj(sock):
    header = recvall(sock, 4)
    if not header: return None
    (size,) = struct.unpack("!I", header)
    return pickle.loads(recvall(sock, size))

def recvall(sock, n):
    buf = b""
    while len(buf) < n:
        chunk = sock.recv(n - len(buf))
        if not chunk: return None
        buf += chunk
    return buf

def connect(host, port, timeout=5.0):
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.settimeout(timeout)
    s.connect((host, port))
    return s
