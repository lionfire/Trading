## Overview

Worker for:
 - Optimization
 - (Future) Bots

## Usage

### Dependencies configuration

- Standalone
  - (FUTURE) Built-in historical data
- LionFire.Trading.Silo
  - Get historical data via Orleans

### Frontend usage options

- Standalone
  - Simple web UI for running optimization
- As worker
  - Use FireLynx.Blazor.Experimental.Host to queue up optimizations and do document management
	
### Clustering usage options
- Standalone
- Orleans cluster
  - (TODO) start multiple in same cluster and optimization jobs can be split among multiple machines
