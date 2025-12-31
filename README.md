# Quantitative Development Projects

This repository is a collection of end-to-end engineering projects exploring how data, algorithms, and infrastructure interact in real systems.

Rather than focusing on a single domain or stack, these projects deliberately span multiple layers of modern software engineering: data pipelines, backend services, distributed computation, containerization, orchestration, testing, and deployment.

The common goal is not feature accumulation, but systems that run, scale, and remain understandable.

## What this repository explores

Across very different use cases, the projects converge on a few core engineering questions:

- How do we turn raw, messy data into reproducible pipelines?
- How do we expose logic through APIs or applications without coupling everything together?
- How do systems behave once computation becomes distributed?
- How do we make results inspectable, testable, and explainable, rather than opaque?
- How do infrastructure choices (Docker, Kubernetes, caching, scheduling) shape system design?

The projects range from data-heavy analytical applications to low-level distributed protocols, and from interactive dashboards to containerized microservices.

## Engineering philosophy

The design principles are consistent across projects:

- **Explicit over implicit**  
  Configuration, parameters, and assumptions are surfaced and documented.

- **Reproducibility over convenience**  
  Pipelines generate stable artifacts, environments are controlled, and results can be regenerated.

- **Interpretability over blind performance**  
  Whether in ML pipelines or distributed systems, understanding behavior matters more than headline metrics.

- **Production realism**  
  Tests, logs, CI/CD, containers, orchestration, and failure modes are treated as first-class concerns.

In short:
> a system is only interesting if we can explain why it behaves the way it does.

## How to navigate this repository

Each project is self-contained and documented at its own level:
architecture, data flow, execution, and deployment live together.

There is no single “entry point, projects can be read independently depending on a reader interests:
application development, backend systems, data engineering, distributed computing, or infrastructure.

## A note on scope

Some projects are intentionally *small and focused* (to isolate a pattern or concept),
others are *broad and system-level* (to explore interactions between components).

This diversity is deliberate: it reflects an interest in **understanding systems across scales**, from a single algorithm to a fully deployed platform.

All projects requiring multiple nodes are executed on a Linux-based virtual machine fleet provided by my shool for educational purposes, deployed and operated on a private OpenStack cloud, with infrastructure provisioning and configuration managed using Terraform and Ansible.

---

**Alexandre Mathias Donnat**
