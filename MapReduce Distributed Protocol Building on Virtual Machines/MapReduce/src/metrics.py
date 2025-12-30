import time

class Timer:
    def __init__(self): self.t0=None; self.t1=None
    def start(self): self.t0=time.monotonic()
    def stop(self): self.t1=time.monotonic()
    @property
    def elapsed(self): return (self.t1 - self.t0) if (self.t1 and self.t0) else 0.0
