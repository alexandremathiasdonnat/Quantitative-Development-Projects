FROM ubuntu:26.04

ENV DEBIAN_FRONTEND=noninteractive
ENV PATH="/usr/local/bin:/root/.local/bin:/root/.zeroclaw/bin:/usr/local/.cargo/bin:${PATH}"

RUN apt-get update && apt-get install -y \
    ca-certificates \
    curl \
    git \
    python3 \
    bash \
    coreutils \
    findutils \
    tar \
    gzip \
    xz-utils \
    unzip \
    build-essential \
    pkg-config \
    libssl-dev \
    && rm -rf /var/lib/apt/lists/*

# Real agent runtime: ZeroClaw.
# The build fails if ZeroClaw is not really installed.
RUN curl -fsSL https://raw.githubusercontent.com/zeroclaw-labs/zeroclaw/master/install.sh -o /tmp/zeroclaw-install.sh \
    && chmod +x /tmp/zeroclaw-install.sh \
    && /tmp/zeroclaw-install.sh --prebuilt --skip-quickstart --apps none --prefix /usr/local \
    && rm -f /tmp/zeroclaw-install.sh

RUN command -v zeroclaw && zeroclaw --version

RUN useradd -m -u 10001 agentuser

WORKDIR /workspace

COPY agent/attack_driver.py /opt/agent/attack_driver.py

CMD ["python3", "/opt/agent/attack_driver.py"]
