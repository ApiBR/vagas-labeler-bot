---
title: Getting Started
nav_order: 2
has_children: true
permalink: /getting-started/
---

# 🚀 Getting Started

Everything you need to build and run Vagas Labeler Bot locally.

---

## In this section

| Page | What you'll learn |
| :--- | :---------------- |
| [Installation]({{ site.baseurl }}/getting-started/installation/) | Cloning, building, testing, and running the three entry points |
| [Configuration]({{ site.baseurl }}/getting-started/configuration/) | Environment variables and `appsettings.json` keys each entry point reads |

---

## Prerequisites

| Requirement | Minimum version | Notes |
| :---------- | :-------------- | :---- |
| .NET SDK | 10.x | Pinned in [`global.json`](https://github.com/ApiBR/vagas-labeler-bot/blob/main/global.json) |
| MariaDB | 10.x | Only needed once the persistence layer is wired up — see [Roadmap]({{ site.baseurl }}/roadmap/) |
| LavinMQ (or RabbitMQ) | any AMQP 0.9.1-compatible broker | Same caveat as MariaDB |
| Git | any | To clone the repository |

{: .note }
As of today the repository is at the **skeleton** phase (see [Roadmap]({{ site.baseurl }}/roadmap/)) — `dotnet build` and `dotnet test` are all you need to get a green build. MariaDB and LavinMQ become relevant once the ingress/worker pipeline lands.
