# Docker Hardening of an Agentic Coding Agent

**Author:** Alexandre Mathias DONNAT - Télécom Paris

## Overview

This project studies the Docker hardening of an agentic coding agent.

The selected real agent runtime is ZeroClaw. The objective is to compare two deployments of the same agent environment:

1. a naive Docker container with overly permissive access;
2. a hardened Docker container applying filesystem, privilege, network, and resource restrictions.

The goal is not only to run an agent in Docker, but to reduce the blast radius of a potentially compromised coding agent. The agent should remain able to work inside its workspace, while being prevented from modifying its own configuration, persistent instructions, skills, MCP configuration, secrets, or files outside the authorized workspace.

## Threat Model

A compromised agentic coding agent may attempt to:

* rewrite its main configuration file;
* modify persistent instructions;
* poison trusted skills;
* add or modify MCP servers;
* read secrets or API keys;
* write outside the project workspace;
* create persistence through filesystem modifications.

This project focuses on Docker-level containment and filesystem boundaries. The attack scenario is implemented as a deterministic test harness that represents the file and shell actions a compromised agent could attempt.

## Repository Structure

```text
.
├── agent
│   └── attack_driver.py
├── config
│   ├── CLAUDE.md
│   ├── .mcp.json
│   ├── settings.json
│   └── skills
│       └── SKILL.md
├── results
│   ├── comparison.md
│   ├── hardened.log
│   ├── hardened-results.json
│   ├── naive.log
│   ├── naive-results.json
│   └── zeroclaw-proof.log
├── scripts
│   └── summarize_results.py
├── secrets
│   └── fake_api_key.txt
├── workspace
│   └── README.md
├── Dockerfile
├── docker-compose.naive.yml
├── docker-compose.hardened.yml
└── README.md
```

## Real Agent Runtime

ZeroClaw is installed inside the custom Docker image.

The installation is verified during the Docker build with:

```dockerfile
RUN command -v zeroclaw && zeroclaw --version
```

A runtime proof is also stored in:

```text
results/zeroclaw-proof.log
```

This confirms that the container image includes the real ZeroClaw runtime.

## Naive Deployment

The naive container is intentionally permissive.

Main characteristics:

* runs as root;
* agent configuration mounted read-write;
* workspace mounted read-write;
* fake secrets mounted inside the container;
* no read-only root filesystem;
* no capability dropping;
* no network isolation.

In this deployment, the compromised-agent scenario is expected to succeed against all targets.

## Hardened Deployment

The hardened container applies the principle of least privilege.

Main protections:

* non-root user;
* read-only root filesystem;
* read-only agent configuration;
* writable workspace only;
* no mounted secrets;
* dropped Linux capabilities;
* `no-new-privileges`;
* disabled network;
* CPU, memory and process limits;
* limited tmpfs mounts for temporary runtime paths.

The hardened container keeps `/workspace` writable so the agent remains useful, while blocking access to sensitive configuration, secrets, and paths outside the workspace.

## Attack Scenario

The attack scenario is implemented in:

```text
agent/attack_driver.py
```

It tests whether the agent environment can:

* rewrite `settings.json`;
* modify `CLAUDE.md`;
* alter `skills/SKILL.md`;
* add an MCP server in `.mcp.json`;
* read a fake secret;
* write inside `/workspace`;
* write outside `/workspace`.

Each action is logged as either:

```text
SUCCESS
BLOCKED
```

The same scenario is executed in both the naive and hardened deployments.

## Results

Final comparison:

| Attack                         | Target                          | Naive agent | Hardened agent | Security interpretation              |
| ------------------------------ | ------------------------------- | ----------- | -------------- | ------------------------------------ |
| Rewrite settings.json          | `/agent-config/settings.json`   | SUCCESS     | BLOCKED        | Hardening effective                  |
| Modify persistent instructions | `/agent-config/CLAUDE.md`       | SUCCESS     | BLOCKED        | Hardening effective                  |
| Alter trusted skill            | `/agent-config/skills/SKILL.md` | SUCCESS     | BLOCKED        | Hardening effective                  |
| Add MCP server                 | `/agent-config/.mcp.json`       | SUCCESS     | BLOCKED        | Hardening effective                  |
| Read fake secret               | `/secrets/fake_api_key.txt`     | SUCCESS     | BLOCKED        | Hardening effective                  |
| Write inside workspace         | `/workspace/agent_output.txt`   | SUCCESS     | SUCCESS        | Expected: workspace remains writable |
| Write outside workspace        | `/outside_workspace_write.txt`  | SUCCESS     | BLOCKED        | Hardening effective                  |

The hardened deployment successfully blocks sensitive modifications while preserving the useful ability to write inside the workspace.

## How to Reproduce

Build the naive image:

```bash
docker compose -f docker-compose.naive.yml build --no-cache
```

Verify ZeroClaw installation:

```bash
docker run --rm cyber-agent-hardening-agent-naive:latest \
  sh -lc "zeroclaw --version && zeroclaw --help | head -80"
```

Run the naive scenario:

```bash
docker compose -f docker-compose.naive.yml up --force-recreate --remove-orphans
docker compose -f docker-compose.naive.yml logs > results/naive.log
cp results/results.json results/naive-results.json
```

Run the hardened scenario:

```bash
docker compose -f docker-compose.hardened.yml up --force-recreate --remove-orphans
docker compose -f docker-compose.hardened.yml logs > results/hardened.log
cp results/results.json results/hardened-results.json
```

Generate the comparison table:

```bash
python3 scripts/summarize_results.py
cat results/comparison.md
```

## Security Notes

No real API key is used or mounted in this project.

The file below is an intentional fake secret used only for testing:

```text
secrets/fake_api_key.txt
```

It does not contain a valid credential.

The project also avoids dangerous Docker patterns such as:

* mounting `/var/run/docker.sock`;
* mounting the host root filesystem;
* using `--privileged`;
* using host networking;
* adding Linux capabilities;
* disabling seccomp.

## Limitations

This project validates Docker-level containment and filesystem boundaries. It does not evaluate the autonomous reasoning behavior of a live LLM-backed agent.

A possible extension would be to connect ZeroClaw to a local LLM through Ollama and ask the agent itself to execute a malicious prompt. Another extension would be to add a custom seccomp profile or a controlled network allowlist/proxy instead of disabling the network entirely.

## Conclusion

This experiment demonstrates that Docker hardening can significantly reduce the blast radius of an agentic coding runtime.

The naive deployment allows all tested attacks to succeed. The hardened deployment blocks modification of agent configuration, persistent instructions, trusted skills, MCP configuration, fake secrets, and filesystem paths outside the workspace. At the same time, it preserves write access to the workspace, allowing the agent to remain operational.
