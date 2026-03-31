# Rapport détaillé — Pipeline Data Lake Géopolitique & Pétrole

**Projet :** Geopolitics & Oil Data Project  
**Date :** 04 mars 2026  
**Équipe :** Omar Fekih-Hassen · Alexandre Donnat · Leo Ivars

## Table des matières

1. Résumé exécutif  
2. Contexte et problématique  
3. Objectifs du projet  
4. Architecture globale  
5. Infrastructure et déploiement local  
6. Orchestration Airflow (cœur opérationnel)  
7. Étape 1 — Ingestion (Bronze)  
8. Étape 2 — Transformation Silver (PySpark)  
9. Étape 3 — Combinaison Gold & Oil Stress Index  
10. Étape 4 — Indexation Elasticsearch  
11. Étape 5 — Dashboard Kibana automatisé  
12. Exécution opérationnelle  
13. Qualité logicielle et tests  
14. Limites observées et risques techniques  
15. Perspectives d’amélioration  
16. Conclusion générale  
Annexe A — Emplacements d’images recommandés

---

## 1) Executive Summary

The project implements a complete data architecture enabling high-frequency (15-minute) analysis of the relationship between global geopolitical stress and WTI crude oil price movements.

The approach follows a classic Data Engineering logic across 5 functional layers:

1. **Collect** market data (Yahoo Finance) and geopolitical news (GDELT),
2. **Clean / standardize** these data,
3. **Combine** signals to produce a synthetic indicator (Oil Stress Index),
4. **Serve** data to an analytics engine (Elasticsearch),
5. **Visualize** results via an automatically deployed Kibana dashboard.

The system is orchestrated with Airflow in two modes:

- **Initialization mode (`init`)**: historical backfill + creation of analytical artifacts,
- **Daily mode (`daily`)**: incremental, idempotent updates.

This architecture addresses requirements for traceability, reproducibility, and continuous data exploitation for business analysis and modeling.

---

## 2) Context and Problem Statement

The oil market reacts to macroeconomic, financial, and geopolitical factors. Among these, international events (conflicts, sanctions, diplomatic tensions, supply disruptions) can create anticipatory shocks to supply and demand.

The methodological problem is twofold:

- market data (WTI) is **discontinuous** (night closures and weekends),
- event data (GDELT) is **continuous** (24/7).

The primary challenge of the pipeline is aligning these timescales to produce a coherent, analytically exploitable signal.

---

## 3) Project Objectives

## 3.1 General Objective

Build an industrializable local Data Lake that transforms two heterogeneous sources into a Gold table ready for:

- decision-making visualization,
- statistical analysis,
- predictive model training.

## 3.2 Specific Objectives

- Implement robust historical and daily ingestion,
- Standardize schemas and types in the Silver layer,
- Define an explainable geopolitical score,
- Design smoothing logic for market closure periods,
- Provide an immediately usable dashboard for the team.

---

## 4) Global Architecture

## 4.1 Data Flow Overview

`Yahoo Finance (WTI, 15m) + GDELT v2 (events, 15m) -> S3 raw -> S3 formatted (Silver) -> S3 combined (Gold) -> Elasticsearch -> Kibana`

#image architecture pipeline here

## 4.2 Component Roles

- **LocalStack (S3)**: local object storage simulating AWS S3.
- **PySpark**: data transformation and combination engine.
- **Airflow**: workflow orchestration and scheduling.
- **Elasticsearch**: search/real-time aggregation indexing.
- **Kibana**: visual results presentation.
- **Poetry**: dependency management and environment reproducibility.

## 4.3 Data Lake Layer Organization

- **Raw (Bronze)**: raw data, close to sources.
    - `raw/gdelt/history/`, `raw/gdelt/daily/`
    - `raw/yahoofinance/history/`, `raw/yahoofinance/daily/`
- **Silver (Formatted)**: cleaned, cast, enriched data.
    - `formatted/gdelt/events.parquet`
    - `formatted/yahoofinance/wti.parquet`
- **Gold (Combined)**: business-value table.
    - `combined/stress_index/`

This segmentation facilitates maintenance, incident recovery, and step-by-step quality control.

---

## 5) Infrastructure and Local Deployment

The Docker environment contains:

- `postgres` for Airflow,
- `localstack` for S3,
- `elasticsearch` and `kibana` for analytics,
- `airflow` (webserver + scheduler).

The `infrastructure/localstack_init.sh` script creates the `datalake` bucket and folder prefixes. The approach is idempotent: restarting services does not destroy existing data.

### 5.1 Useful Ports and Access

- Airflow: `http://localhost:8080`
- LocalStack S3: `http://localhost:4566`
- Elasticsearch: `http://localhost:9200`
- Kibana: `http://localhost:5601`

#image docker compose / services up here

---

## 6) Airflow Orchestration (Operational Core)

The file `dags/main_pipeline_dag.py` defines two complementary DAGs.

## 6.1 DAG `oil_geopolitics_init` (Manual, One-Shot)

This DAG bootstraps the system:

1. `backfill_gdelt`
2. `backfill_yfinance`
3. `clean_gdelt --mode history`
4. `clean_yfinance --mode history`
5. `compute_stress_index --mode history`
6. `index_to_elastic`
7. `setup_kibana`

It creates the reference history and generates the initial dashboard.

## 6.2 DAG `oil_geopolitics_daily` (Scheduled, Incremental)

Schedule: `0 8 * * *` UTC.  
Daily chain:

1. `batch_extract_gdelt --date {{ ds }}`
2. `batch_extract_yfinance --date {{ ds }}`
3. `clean_gdelt --mode daily --date {{ ds }}`
4. `clean_yfinance --mode daily --date {{ ds }}`
5. `compute_stress_index --mode daily --date {{ ds }}`
6. `index_to_elastic`

Using `{{ ds }}` standardizes processing date across the entire pipeline.

## 6.3 Why Two DAGs?

- **init**: expensive, rarely executed, builds the baseline state,
- **daily**: fast, targeted, maintains current state without full recalculation.

#image airflow dag graph here

---

## 7) Step 1 — Ingestion (Bronze)

## 7.1 WTI Ingestion (Yahoo Finance)

Scripts:

- Historical: `src/ingestion/backfill_yfinance.py`
- Daily: `src/ingestion/batch_extract_yfinance.py`

### Applied Logic

- retrieval of `CL=F` ticker at 15-minute intervals,
- DataFrame conversion and column normalization,
- Parquet export to S3,
- handling of no-data days (market closure).

### Key Point

Yahoo Finance imposes a depth constraint on 15m intraday data (approximately 60 days). The `init` DAG handles this with a dynamically adapted start date.

## 7.2 GDELT Ingestion

Scripts:

- Historical: `src/ingestion/backfill_gdelt.py`
- Daily: `src/ingestion/batch_extract_gdelt.py`

### Applied Logic

- retrieval of GDELT master file list (historical),
- generation of 96 timestamps per day (daily),
- download of `*.export.CSV.zip` archives,
- tabular conversion to fixed 58-column schema,
- Parquet export to S3.

### Robustness

Missing files (404) are logged then ignored in daily mode, preventing complete pipeline failure for an absent slot.

#image raw s3 overview here

---

## 8) Step 2 — Silver Transformation (PySpark)

The Silver layer converts heterogeneous data into clean, analytically reliable tables.

## 8.1 Silver GDELT (`src/transformation/clean_gdelt.py`)

### Technical Cleaning

- parse `DATEADDED` and `Day`,
- cast numeric columns,
- remove duplicates by `GlobalEventID`,
- temporal sort.

### Business Event Filtering

The code retains only significant events via thresholds:

- `NumArticles >= 4` (media coverage level),
- `|GoldsteinScale| >= 5` (diplomatic/conflict intensity),
- `QuadClass >= 2` (relevant event categories),
- targeted `EventRootCode`,
- study period from 01/01/2026 onwards.

### Geopolitical Score Construction

- `geo_I`: importance of involved actors/countries,
- `geo_B`: media amplification,
- `geo_S`: event severity,
- `geo_score_raw`: multiplicative combination.

The script also builds `actor_countries` (deduplicated ISO3 country list) for actor analysis and Kibana mapping.

## 8.2 Silver WTI (`src/transformation/clean_yfinance.py`)

### Technical Cleaning

- cast OHLCV,
- UTC harmonization,
- remove temporal duplicates on `Datetime`.

### Market Enrichment

- `Volatility_Range = High - Low`: intrabar amplitude,
- `Variation_Pct`: relative variation vs previous close.

These variables provide finer market reaction reading beyond close price alone.

#image example silver dataframe here

---

## 9) Step 3 — Gold Combination & Oil Stress Index

Script: `src/combination/compute_stress_index.py`

This step is most business-critical: it transforms Silver signals into an exploitable synthetic indicator.

## 9.1 Problem Addressed

GDELT is continuous (24/7), WTI is not. Without correction, weekend events would be invisible until market reopening. The code implements **reporting/smoothing** logic to the next market opening.

## 9.2 Gold Pipeline Details

1. **15m GDELT Aggregation**
     - sum of `geo_*` scores,
     - event counting,
     - preservation of dominant event's countries per slot.

2. **Full Outer Join WTI × GDELT**
     - timestamp alignment,
     - creation of `market_open` flag.

3. **Forward Mapping**
     - each closed interval points to next open candle (`target_open_datetime`).

4. **Closed Period Smoothing**
     - hybrid calculation: average + peak weighting,
     - parameter `alpha = 0.25`.

5. **7-Day Normalized Score**
     - `score_pct_7d` calculation in rolling window,
     - production of interpretable scale (0 to 100).

## 9.3 Gold Output Schema

The final table contains:

- market metrics (OHLCV + volatility + variation),
- smoothed geopolitical scores,
- raw aggregate scores,
- event count and gap duration,
- dominant actor (`period_actor_country`),
- 7-day percentile (`score_pct_7d`).

This table is directly exploitable for BI and ML.

#image stress index vs wti chart here

---

## 10) Step 4 — Elasticsearch Indexing

Script: `src/indexing/load_to_elastic.py`

## 10.1 Process

1. read Gold Parquet from S3,
2. convert `Datetime` to ISO UTC `timestamp`,
3. idempotent creation of `oil-market-analysis` index,
4. bulk send in batches.

## 10.2 Value of Explicit Mapping

The mapping (`config/elastic_mapping.json`) guarantees correct types (`date`, `float`, `long`, `keyword`) and prevents dynamic inference errors on Elasticsearch/Kibana side.

## 10.3 Key Field for Temporal Analysis

The `timestamp` field serves as unique temporal reference in Kibana, essential for consistent visualizations and period filters.

#image elastic index / discover here

---

## 11) Step 5 — Automated Kibana Dashboard

Script: `src/visualization/setup_kibana.py`

The script automatically creates:

- a Data View,
- Lens visualizations,
- a choropleth map,
- markdown section panels,
- a structured final dashboard.

## 11.1 Automation Value

The team can rebuild the dashboard without manual actions: useful for demonstrations, infrastructure reset, or project reproduction on another machine.

## 11.2 Business Reading of Visuals

- **WTI Close**: market trend,
- **Volume**: exchange intensity,
- **Variation vs geopolitical score**: joint dynamics,
- **I/B/S Scores**: stress driver decomposition,
- **Top actors + map**: geography of dominant tensions.

#image kibana dashboard global here
#image kibana top actors panel here
#image kibana actors map here

---

## 12) Operational Execution

## 12.1 Initialization (One-Shot)

1. launch Docker infrastructure,
2. execute historical ingestion,
3. run history transformations,
4. compute Gold,
5. index into Elasticsearch,
6. deploy Kibana dashboard.

## 12.2 Daily Execution

The `oil_geopolitics_daily` DAG chains extraction, cleaning, combination, and indexing for date `{{ ds }}`.

## 12.3 Idempotence

Most steps overwrite/rewrite the target partition or day, allowing date replay without undesired accumulation.

#image airflow run success here

---

## 13) Software Quality and Tests

The project includes a suite of unit tests by critical module:

- ingestion (`test_backfill_*`, `test_batch_extract_*`),
- transformation (`test_clean_*`),
- combination (`test_compute_stress_index.py`),
- indexing (`test_load_to_elastic.py`),
- visualization (`test_setup_kibana.py`).

Quality Tools:

- `pytest`, `pytest-cov`,
- `ruff` (lint),
- `black` (format).

This foundation improves evolution reliability and reduces regression risk.

---

## 14) Observed Limitations and Technical Risks

1. **Limited Yahoo Intraday Window**: external source constraint.
2. **Local Architecture**: LocalStack is perfect for dev/POC but requires cloud adaptation (IAM, real S3, monitoring) for production.
3. **Spark Overwrite Cost**: some steps can be optimized (partitioning, incremental merge, compaction).
4. **Network/API Resilience**: add standardized retries/backoff and alerting.
5. **Schema Governance**: plan automatic schema validation at each ingestion.

---

## 15) Improvement Perspectives

## 15.1 Data Engineering

- partition Gold by date,
- finer incremental management (upsert),
- quality metadata (completeness, freshness, duplicates).

## 15.2 Observability

- Airflow alerts (failure, abnormal duration),
- technical dashboards (pipeline latency, ingested volume, errors by source),
- centralized structured logging.

## 15.3 Data Science / Business

- advanced geopolitical score calibration,
- out-of-sample robustness testing,
- enrichment with macro variables (USD index, rates, stocks).

## 15.4 Architecture Roadmap

A natural extension is activating the streaming phase (Kafka) already sketched in infrastructure, to reduce latency between geopolitical event and visualization.

---

## 16) General Conclusion

The project proposes a solid and coherent implementation of a modern analytical pipeline:

- clear layered architecture (Bronze/Silver/Gold),
- operational orchestration (init + daily),
- relevant business logic for aligning market/event timescales,
- automated visual restitution.

The foundation is mature enough for a convincing group deliverable and modular enough to evolve toward quasi-production usage.

---

## Appendix A — Recommended Image Placements

You can insert your screenshots at the following markers:

1. `#image architecture pipeline here`
2. `#image docker compose / services up here`
3. `#image airflow dag graph here`
4. `#image raw s3 overview here`
5. `#image example silver dataframe here`
6. `#image stress index vs wti chart here`
7. `#image elastic index / discover here`
8. `#image kibana dashboard global here`
9. `#image kibana top actors panel here`
10. `#image kibana actors map here`
11. `#image airflow run success here`

