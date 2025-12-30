#  Multi-Signal Recipe Classifier (Structural x NLP)

**Objective:** Classify recipes into **`plat` / `dessert` / `boisson`** by combining quantitative nutritional features and textual cues through a dual-phase model followed by an explicit arbitration layer.  
The entire process is designed to be interpretable, numerically stable, and reproducible.

![alt text](figure.png)

##  Data

**Source:** [Food.com Recipes & Interactions (Kaggle)](https://www.kaggle.com/datasets/shuyangli94/food-com-recipes-and-user-interactions?select=RAW_interactions.csv)  
We use `INPUT_rawrecipes.csv` as the primary dataset (over 200k+ unique lines). CSV's are attached but voluntary reduced for git support.

##  Method 

The pipeline consists of **four sequential phases**, moving from raw nutritional data to a final decision obtained through a rule-based stacking mechanism. This repo contains the .ipynb EDA , and the .py  identicals but more easily executable.

###  Phase 0 — Structural Features (Nutrition Parsing)

We parse the field $\text{nutrition} \rightarrow [\text{cal}, \text{fat}, \text{sugar}, \text{sod}, \text{prot}, \text{sat}, \text{carbs}]$
and construct normalized, calorie-scaled variables.

**Initial Formulas:**
```yaml
sugar_density = sugar / (cal + ε) 
prot_density  = prot  / (cal + ε)
sod_density   = sod   / (cal + ε)
```

Energy contributions:
```yaml
fat_E%  = 9 * fat   / cal
carb_E% = 4 * carbs / cal
prot_E% = 4 * prot  / cal
```

Composite indicators:
```yaml
sweet_idx  = 0.55 * sugar_E%   + 0.45 * sugar_density
savory_idx = 0.55 * prot_density + 0.45 * (sod_density / 10)
hybrid_idx = min(sweet_idx, savory_idx)
lean_idx   = 1 - fat_E%
```

All metrics are bounded and ε-regularized to avoid numerical instability.

###  Phase 1 — Structural Prototype Classifier

Each recipe is embedded in a 3D flavor-energy space $(\text{sweet}, \text{savory}, \text{lean})$  
and compared to **fixed class prototypes**:

| Class | Sweet | Savory | Lean |
|:------|:------:|:------:|:----:|
| Dessert | 0.68 | 0.07 | 0.40 |
| Plat | 0.12 | 0.28 | 0.45 |
| Boisson | 0.09 | 0.05 | 0.85 |

Classification is based on **cosine similarity** to each prototype, followed by heuristic corrections (e.g. high-sugar + low-sodium → dessert, lean + low-protein → boisson).  
Probabilities are derived via **softmax**, and confidence is estimated from the **probability margin** and **entropy**.

###  Phase 2 — NLP (Name + Tags)

We extract lexical information from `name` and `tags` using two dictionaries:
- **STRONG lexicon:** decisive terms (presence → 0/1)
- **SOFT lexicon:** suggestive terms (counts)

$$
\text{logits} = \alpha \cdot \text{STRONG} + \beta \cdot \text{SOFT} + 0.1, 
\quad (\alpha=3.0,\ \beta=0.8)
$$

The result is normalized through softmax.  
Example signals:  
- “stew”, “curry”, “chili” → `plat`  
- “cake”, “brownie”, “cheesecake” → `dessert`  
- “smoothie”, “latte”, “milkshake” → `boisson`

###  Phase 3 — Explicit Arbiter (Mini-Stacking)

Phases 1 and 2 are treated as **two independent classifiers**.  
This phase introduces an **explicit arbitration mechanism** to combine both probabilistic outputs.

**Procedure:**
1. Compute an **NLP vote** (label + strength level) from STRONG and SOFT hits.  
2. Compare with structural class and confidence.  
3. Blend probabilities according to agreement and confidence hierarchy.

**Rules:**
- Strong structural confidence → structure dominates, NLP adds minor adjustment.  
- Weak structural confidence → NLP gains weight or replaces the label.  
- Hard-coded exceptions:  
  - ID-based fixes (verified ground truth).  
  - Smoothie/milkshake → forced `boisson`.

Confidence is recalculated from the blended probabilities using entropy and margin-based calibration.

##  Outputs

Each recipe receives:
- `p_struct_*`, `p_nlp_*`, `p_final_*` → probability vectors  
- `type` → final predicted class  
- `conf_%` → calibrated confidence  
- optional `exception_hit` flag



