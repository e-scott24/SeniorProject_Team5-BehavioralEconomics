/*  Name: Jason Black
    Date: 2/23/2026
    Last Update: 4/8/2026

    Dave Ramsey budget calculator logic.
    Handles field generation, live calculations, donut chart, and validation.

    
    Calculator logic is in calculator.js.
    All markup/CSS is in _Calculator.cshtml. 
    Refer to FinancialsController.cs for the APIs. 
    Layout and open/close logic are in _Layout.cshtml.
    Program.cs was altered to support API controllers, session, and distributed memory cache.
    All icons are from https://heroicons.com/.

    The correct file paths are the following:
    ...\DealtHands\DealtHands\wwwroot\js\calculator.js
    ...\DealtHands\DealtHands\Pages\Shared\_Calculator.cshtml
    ...\DealtHands\DealtHands\Controllers\FinancialsController.cs
    ...\DealtHands\DealtHands\Pages\Shared\_Layout.cshtml
    ...\DealtHands\DealtHands\Program.cs


    Difficulty levels:
    Easy:   Take-Home Pay / Student Loans, Car, House, Family, Food, Consumer Debt, Entertainment, Other/Misc, Savings
    Medium: TBD — define MEDIUM_INCOME and MEDIUM_EXPENSES below when ready
    Hard:   TBD — define HARD_INCOME and HARD_EXPENSES below when ready

    Income/Expenses information:
    hint = smaller text under the label for extra guidance
    tip = tooltip text that appears when hovering over the "?" icon next to the label, meant for more in-depth advice
    pctLabel = the recommended percentage range shown in the label (e.g. "10–15%")
*/



(function () {
    "use strict";

    const EASY_INCOME = [
        {
            key: "income",
            label: "Take-Home Pay (after taxes)",
            hint: "Your actual deposited amount each month",
        },
    ];

    const EASY_EXPENSES = [
        {
            key: "studentloans",
            label: "Student Loans",
            color: "#6c63ff",
            pctLabel: "5–10%",
            hint: "Federal and private loan monthly payments",
            tip: "Federal and private student loan payments only. This is one of the most dangerous long-term debts. Focus on paying it off aggressively once your starter emergency fund is in place.",
            warnPct: 0.10
        },
        {
            key: "car",
            label: "Car",
            color: "#4cc9f0",
            pctLabel: "10–15%",
            hint: "Payment, insurance, fuel, and maintenance",
            tip: "Includes car payment, insurance, fuel, and maintenance. Transportation should stay reasonable so it doesn't dominate your budget.",
            warnPct: 0.15
        },
        {
            key: "house",
            label: "House",
            color: "#3a86ff",
            pctLabel: "25–35%",
            hint: "Monthly rent or mortgage payment",
            tip: "Housing is usually the largest expense, so keeping it within a healthy range protects the rest of your budget from being squeezed.",
            warnPct: 0.35
        },
        {
            key: "family",
            label: "Family",
            color: "#ff9f1c",
            pctLabel: "5–10%",
            hint: "Pets, relatives, and everything household related",
            tip: "Family expenses can be unpredictable, so planning for them helps prevent surprises.",
        },
        {
            key: "food",
            label: "Food",
            color: "#e9c46a",
            pctLabel: "10–15%",
            hint: "Groceries and dining out combined",
            tip: "Groceries and dining out combined. Cooking at home and planning meals can dramatically reduce spending.",
            min: 200
        },
        {
            key: "debt",
            label: "Consumer Debt",
            color: "#e63946",
            pctLabel: "0–10%",
            hint: "Credit cards, medical bills, and personal loans",
            tip: "Credit cards, medical bills, personal loans, and buy-now-pay-later balances. All of your other debts.",
            warnPct: 0.15
        },
        {
            key: "entertainment",
            label: "Entertainment",
            color: "#e76f51",
            pctLabel: "0–5%",
            hint: "Streaming, hobbies, and fun spending",
            tip: "Streaming services, hobbies, outings, and fun spending. This category keeps life enjoyable, but it should stay small.",
            warnPct: 0.05
        },
        {
            key: "other",
            label: "Other / Misc",
            color: "#8ab4c4",
            pctLabel: "5–10%",
            hint: "Everything that doesn't fit elsewhere",
            tip: "Personal care and anything else. This category catches expenses that don't fit anywhere else so every dollar still has a purpose.",
            warnPct: 0.10
        },
        {
            key: "savings",
            label: "Savings",
            color: "#2a9d8f",
            pctLabel: "10–15%",
            hint: "Emergency fund and long-term savings",
            tip: "Build a starter emergency fund first, then grow long-term savings and retirement. Saving creates stability and protects you from sudden expenses."
        },
    ];

    const MEDIUM_INCOME = [];
    const MEDIUM_EXPENSES = [];
    const HARD_INCOME = [];
    const HARD_EXPENSES = [];

    let activeIncome = EASY_INCOME;
    let activeExpenses = EASY_EXPENSES;

    const EF_GOAL = 1000;
    const CIRC = 2 * Math.PI * 60;
    const MAX_INPUT = 999999;

    const $ = id => document.getElementById(id);

    function setActiveDifficulty(difficulty) {
        switch (difficulty) {
            case "medium":
                activeIncome = MEDIUM_INCOME;
                activeExpenses = MEDIUM_EXPENSES;
                break;
            case "hard":
                activeIncome = HARD_INCOME;
                activeExpenses = HARD_EXPENSES;
                break;
            default:
                activeIncome = EASY_INCOME;
                activeExpenses = EASY_EXPENSES;
        }
    }

    function getVal(id) {
        var el = $(id);
        if (!el) return 0;
        var v = parseFloat(el.value);
        if (isNaN(v) || v < 0) return 0;
        if (v > MAX_INPUT) {
            el.value = MAX_INPUT;
            return MAX_INPUT;
        }
        return v;
    }

    function fmt(n) {
        return "$" + Math.abs(n).toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    }

    function buildIncomeFields() {
        var container = $("c-income-fields");
        if (!container) return;
        container.innerHTML = "";
        activeIncome.forEach(function (src) {
            var tipHtml = src.tip
                ? '<span class="calc-tip">?<span class="calc-tip-text">' + src.tip + '</span></span>'
                : '';
            var div = document.createElement("div");
            div.className = "calc-field";
            div.innerHTML =
                '<div class="calc-field-label">' +
                '  ' + src.label + tipHtml +
                '  <small>' + (src.hint || '') + '</small>' +
                '</div>' +
                '<div class="calc-field-input-wrap">' +
                '  <div class="calc-input-wrap">' +
                '    <span class="calc-dollar">$</span>' +
                '    <input type="number" inputmode="decimal" id="c-' + src.key + '" min="0" step="1" placeholder="0.00"/>' +
                '  </div>' +
                '  <div class="calc-field-err" id="c-err-' + src.key + '"></div>' +
                '</div>';
            container.appendChild(div);
        });
    }

    function buildExpenseFields() {
        var container = $("c-expense-fields");
        if (!container) return;
        container.innerHTML = "";
        activeExpenses.forEach(function (expense) {
            var drBadge = '<span class="calc-rec-badge">Recommended: ' + expense.pctLabel + '</span>';
            var hint = (expense.hint || '') + ' ' + drBadge;

            var div = document.createElement("div");
            div.className = "calc-field";
            div.innerHTML =
                '<div class="calc-field-label">' +
                '  <div style="display:flex;align-items:center;gap:4px">' +
                '    <span style="width:9px;height:9px;border-radius:50%;background:' + expense.color + ';display:inline-block;flex-shrink:0"></span>' +
                '    ' + expense.label +
                '    <span class="calc-tip">?<span class="calc-tip-text">' + expense.tip + '</span></span>' +
                '  </div>' +
                '  <small>' + hint + '</small>' +
                '</div>' +
                '<div class="calc-field-input-wrap">' +
                '  <div class="calc-input-wrap">' +
                '    <span class="calc-dollar">$</span>' +
                '    <input type="number" inputmode="decimal" id="c-' + expense.key + '" min="' + (expense.min || 0) + '" step="1" placeholder="0.00"/>' +
                '  </div>' +
                '  <div class="calc-field-err" id="c-warn-' + expense.key + '"></div>' +
                '</div>';
            container.appendChild(div);
        });
    }

    function drawDonut(slices, income) {
        var svg = $("c-donut");
        if (!svg) return;

        svg.querySelectorAll(".c-seg").forEach(function (e) { e.remove(); });

        var total = slices.reduce(function (s, sl) { return s + sl.val; }, 0);
        $("c-donut-total").textContent = fmt(total);
        if (!total) return;

        var base = Math.max(income, total);
        var offset = -0.25 * CIRC;

        function addArc(fraction, color) {
            if (fraction <= 0) return;
            var dash = fraction * CIRC;
            var gap = CIRC - dash;
            var c = document.createElementNS("http://www.w3.org/2000/svg", "circle");
            c.setAttribute("class", "c-seg");
            c.setAttribute("cx", "80");
            c.setAttribute("cy", "80");
            c.setAttribute("r", "60");
            c.setAttribute("fill", "none");
            c.setAttribute("stroke", color);
            c.setAttribute("stroke-width", "22");
            c.setAttribute("stroke-dasharray", dash + " " + gap);
            c.setAttribute("stroke-dashoffset", -offset);
            c.style.transition = "stroke-dasharray .4s cubic-bezier(.4,0,.2,1)";
            var texts = svg.querySelectorAll("text");
            svg.insertBefore(c, texts[0]);
            offset += dash;
        }

        slices.forEach(function (sl) { if (sl.val > 0) addArc(sl.val / base, sl.color); });

        var rem = income - total;
        if (rem > 1 && income > 0) addArc(rem / base, "#e2e8f0");
    }

    function renderSummary(slices, income) {
        var el = $("c-summary-rows");
        if (!el) return;
        el.innerHTML = "";
        slices.forEach(function (sl) {
            if (sl.val === 0) return;
            var pct = income > 0 ? ((sl.val / Math.max(income, 1)) * 100).toFixed(1) + "%" : "–";
            var row = document.createElement("div");
            row.className = "calc-summary-row";
            row.innerHTML =
                '<span class="calc-row-label">' +
                '  <span class="calc-dot" style="background:' + sl.color + '"></span>' +
                '  ' + sl.label +
                '</span>' +
                '<span style="font-size:.75rem;color:#94a3b8;margin-right:.5rem">' + pct + '</span>' +
                '<span class="calc-row-value">' + fmt(sl.val) + '</span>';
            el.appendChild(row);
        });
    }

    function updateStrip(checkingBalance, totalDebt) {
        var debtDisplay = $("c-debt-display");
        var balanceDisplay = $("c-balance-display");

        if (debtDisplay) {
            debtDisplay.textContent = fmt(totalDebt);
            debtDisplay.classList.remove("calc-loading");
            debtDisplay.classList.toggle("calc-value-danger", totalDebt > 0 && totalDebt > checkingBalance);
        }
        if (balanceDisplay) {
            balanceDisplay.textContent = fmt(checkingBalance);
            balanceDisplay.classList.remove("calc-loading");
            balanceDisplay.classList.toggle("calc-value-warn", checkingBalance > 0 && checkingBalance < totalDebt);
        }
    }

    function wireInputs() {
        activeIncome.map(function (src) { return "c-" + src.key; })
            .concat(activeExpenses.map(function (expense) { return "c-" + expense.key; }))
            .forEach(function (id) {
                var el = $(id);
                if (el) el.addEventListener("input", calculate);
            });
    }

    function calculate() {
        var income = activeIncome.reduce(function (sum, src) {
            return sum + getVal("c-" + src.key);
        }, 0);

        var sumEl = $("c-sum-income");
        if (sumEl) sumEl.textContent = fmt(income);

        var slices = activeExpenses.map(function (expense) {
            return { key: expense.key, label: expense.label, color: expense.color, val: getVal("c-" + expense.key) };
        });
        var total = slices.reduce(function (s, sl) { return s + sl.val; }, 0);
        var diff = income - total;

        var diffRow = $("c-diff-row"), diffVal = $("c-diff-val");
        if (diffRow && diffVal) {
            diffRow.className = "calc-diff-row";
            if (Math.abs(diff) < 0.005) {
                diffVal.textContent = "$0.00";
                diffRow.classList.add("balanced");
            } else if (diff > 0) {
                diffVal.textContent = "+" + fmt(diff) + " unallocated";
                diffRow.classList.add("surplus");
            } else {
                diffVal.textContent = "-" + fmt(Math.abs(diff)) + " over";
                diffRow.classList.add("overspent");
            }
        }

        function toggleAlert(id, show) {
            var el = $(id);
            if (el) el.className = el.className.replace(" visible", "") + (show ? " visible" : "");
        }
        toggleAlert("c-alert-surplus", diff > 0.005);
        toggleAlert("c-alert-over", diff < -0.005);
        toggleAlert("c-alert-balanced", Math.abs(diff) < 0.005 && income > 0 && total > 0);

        drawDonut(slices, income);
        renderSummary(slices, income);

        var amountSaved = getVal("c-savings");
        var efPct = Math.min((amountSaved / EF_GOAL) * 100, 100);
        var efBar = $("c-ef-bar");
        var efLabel = $("c-ef-label");
        if (efBar) efBar.style.width = efPct + "%";
        if (efLabel) efLabel.textContent = fmt(amountSaved) + " / " + fmt(EF_GOAL);

        activeExpenses.forEach(function (expense) {
            var warningEl = $("c-warn-" + expense.key);
            var inputEl = $("c-" + expense.key);
            if (!warningEl || !inputEl) return;
            inputEl.classList.remove("calc-warning", "calc-error");
            warningEl.textContent = "";
            warningEl.classList.remove("visible");
            var v = getVal("c-" + expense.key);
            if (expense.min && v > 0 && v < expense.min) {
                warningEl.textContent = "Min $" + expense.min;
                warningEl.classList.add("visible");
                inputEl.classList.add("calc-error");
            } else if (expense.warnPct && income > 0 && v > income * expense.warnPct) {
                warningEl.textContent = "Above " + Math.round(expense.warnPct * 100) + "% limit";
                warningEl.classList.add("visible");
                inputEl.classList.add("calc-warning");
            }
        });
    }

    function loadFinancials() {
        var modal = document.getElementById('calc-model');
        var playerId = modal ? modal.getAttribute('data-player-id') : null;
        var url = playerId ? '/api/financials?playerId=' + playerId : '/api/financials';

        fetch(url)
            .then(function (res) {
                if (!res.ok) throw new Error('financials fetch failed: ' + res.status);
                return res.json();
            })
            .then(function (data) {
                setActiveDifficulty((data && data.difficulty) || "easy");
                buildIncomeFields();
                buildExpenseFields();
                wireInputs();

                if (data && typeof data.monthlyIncome === 'number') {
                    var primaryIncomeInput = $('c-' + activeIncome[0].key);
                    if (primaryIncomeInput) primaryIncomeInput.value = data.monthlyIncome;
                }

                if (data && typeof data.emergencyFundSaved === 'number') {
                    var savingsInput = $('c-savings');
                    if (savingsInput) savingsInput.value = data.emergencyFundSaved;
                }

                updateStrip(
                    (data && data.checkingBalance) || 0,
                    (data && data.totalDebt) || 0
                );

                calculate();
            })
            .catch(function () {
                setActiveDifficulty("easy");
                buildIncomeFields();
                buildExpenseFields();
                wireInputs();
                updateStrip(0, 0);
                calculate();
            });
    }


    // Expose reload function globally
    window.reloadCalculatorData = loadFinancials;

    // Reset button
    var resetBtn = $("c-btn-reset");
    if (resetBtn) {
        resetBtn.addEventListener("click", function () {
            activeIncome.map(function (src) { return "c-" + src.key; })
                .concat(activeExpenses.map(function (expense) { return "c-" + expense.key; }))
                .forEach(function (id) {
                    var el = $(id);
                    if (el) el.value = "";
                });
            activeExpenses.forEach(function (expense) {
                var warningEl = $("c-warn-" + expense.key);
                var inputEl = $("c-" + expense.key);
                if (warningEl) { warningEl.textContent = ""; warningEl.classList.remove("visible"); }
                if (inputEl) inputEl.classList.remove("calc-warning", "calc-error");
            });
            calculate();
        });
    }

}());