# Ecosystem-Sim-AIRS-Project
Advanced Independent Research Seminar Project (2022-2023).

This repository contains the code for my Ecosystem Simulation project. It is built in Unity3D, version 2021.3.18f1.

## Description
This project is an approach to simulating and handling a detailed single-biome ecosystem at large scales. It uses a multi-scale approach to structuring the simulation world and mathematical approximations for real-world mechanics in order to optimize the simulation. This multi-scale data structure approach uses a compound grid system with varying grid sizes, allowing individual simulation systems to be isolated to a specific grid level. A benefit of this data structure is that is is capable of performing calculations on the GPU through the use of Compute Shaders. Furthermore, the data structure is able to perform high-speed neighbor checks and range queries. The simulation uses this data structure to simulate tree growth, water flow, and weather systems that will all be linked together, creating a complex, dynamic, and realistic ecosystem all while performing in real time.

For more information about this project, please see my research proposal linked below:
[**Research Proposal Paper** (pdf)](https://github.com/AIP21/Ecosystem-Sim-AIRS-Project/files/11242417/AIRS.Proposal.pdf)
