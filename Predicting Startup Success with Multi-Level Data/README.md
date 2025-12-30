# Predicting Startup Success with Multi-Level Data

## About
This thesis investigates how startup success can be predicted by combining macroeconomic signals, ecosystem-level patterns, and micro-level company attributes within a unified, data-driven framework.

The work proposes a three-layered methodology that bridges:
- **Macro-level dynamics**, capturing sectoral momentum and emerging trends using Google Trends data;
- **Micro-level structural analysis**, identifying success patterns across global startup ecosystems using Crunchbase data;
- **Supervised machine learning models**, designed to predict startup outcomes (IPO, acquisition, failure) based on enriched microeconomic features.

## Methodology
The research is structured into three complementary layers:

1. **Macro-Sectoral Analysis – Buzz Dynamics**  
   Construction of sector-level momentum indicators derived from multi-horizon Google Trends signals, aggregated into forward-looking “buzz” scores and mapped to strategic investment opportunities.

2. **Micro-Level – Structural analysis**  
   Large-scale descriptive and exploratory analysis of global startups (Crunchbase), focusing on structural attributes such as funding history, timing, sector, and geography to uncover success patterns.

3. **Micro-Level – Predictive Modelling**  
   Supervised learning models applied to a curated U.S. Crunchbase dataset, evaluating multiple preprocessing pipelines and algorithms to predict startup success, with a strong emphasis on interpretability, robustness, and evaluation trade-offs.

## Results & Findings

This research confirms the relevance and robustness of a multi-layered approach to predicting startup success. By combining macro-level sectoral buzz dynamics, micro-level structural startup attributes, and supervised machine learning models, the framework captures the complexity of entrepreneurial ecosystems across multiple analytical scales.

Empirical results show that ensemble-based models such as Random Forest, AdaBoost, and XGBoost consistently outperform simpler approaches, highlighting the importance of modeling nonlinear interactions and heterogeneous effects in startup data.

Several limitations were identified. In particular, the integration of Natural Language Processing (NLP) was constrained by data availability and cost, reflecting a broader challenge in startup analytics: access to rich, real-time textual and sentiment data. While dictionary-based NLP methods provided initial insights, their impact remained limited given current data constraints.

Looking forward, the combination of structured financial and structural data with advanced NLP and alternative data sources (e.g. news, social media, investor communication) represents a promising direction for more timely and nuanced startup success prediction. This work serves as a foundation for such extensions and underscores the growing role of AI-driven decision support in venture capital.


## The contribution
- A novel macro-to-micro framework linking market-level signals to individual startup outcomes  
- Original sectoral “buzz” indicators with predictive and strategic relevance  
- Empirical validation of machine learning models for venture capital decision support  
- A reproducible, data-driven approach to startup success forecasting

## Intended Audience
This paper is designed for:
- Venture capital professionals and innovation analysts  
- Data scientists and quantitative researchers  
- Academics interested in machine learning applications in finance and entrepreneurship
