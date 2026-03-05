/*
  Name: Jason Black
  Date: 2/23/2026
  Last Update: 2/26/2026
  Description: JavaScript for the Dave Ramsey Easy-Mode Zero-Based Budget Calculator.
               Handles live calculation, validation, donut chart, SOS auto-fill,
               category warnings, and reset.
*/

(function () {
    "use strict";

    // ── Config ─────────────────────────────────────────

    const CATS = [
        { key: "studentloans", label: "Student Loans", color: "#6c63ff", pct: 0.10, tip: "DR calls student loans 'the most dangerous debt.' Use the debt snowball in Baby Step 2.", warnPct: null },
        { key: "car", label: "Car", color: "#4cc9f0", pct: 0.10, tip: "Car payment + insurance + gas. DR recommends under 15% of take-home pay.", warnPct: 0.15 },
        { key: "house", label: "House", color: "#3a86ff", pct: 0.25, tip: "Rent or mortgage + utilities. DR recommends 25–35% of take-home pay.", warnPct: 0.35 },
        { key: "family", label: "Family", color: "#ff9f1c", pct: 0.05, tip: "Child care, clothing, school supplies, medical, and other family costs.", warnPct: null },
        { key: "food", label: "Food", color: "#e9c46a", pct: 0.12, tip: "Groceries + dining out. DR suggests 10–15% of take-home. Minimum $50/month.", warnPct: null, min: 50 },
        { key: "debt", label: "Debt", color: "#e63946", pct: 0.05, tip: "Credit cards, medical bills, personal loans. Smallest-to-largest debt snowball.", warnPct: 0.20 },
        { key: "entertainment", label: "Entertainment", color: "#e76f51", pct: 0.05, tip: "Subscriptions, dining out for fun, hobbies. Keep under 5–10%.", warnPct: 0.10 },
        { key: "other", label: "Other / Misc", color: "#8ab4c4", pct: 0.03, tip: "Subscriptions, personal care, gifts, pets. Every dollar must be accounted for.", warnPct: 0.15 },
        { key: "savings", label: "Savings", color: "#2a9d8f", pct: 0.15, tip: "Start with $1,000 emergency fund (Baby Step 1), then 15% for retirement.", warnPct: null },
    ];

    const CIRCUMFERENCE = 2 * Math.PI * 64;

    // ── DOM ────────────────────────────────────────────

    const $ = id => document.getElementById(id);

    // ── Helpers ────────────────────────────────────────

    function fmtDollar(n) {
        return "$" + Math.abs(n).toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    }

    function getVal(id) {
        const el = $(id);
        if (!el) return 0;
        const v = parseFloat(el.value);
        return isNaN(v) ? 0 : Math.max(0, v);
    }

    // ── Build expense fields dynamically ───────────────

    function buildFields() {
        const container = $("expense-fields");
        if (!container) return;

        CATS.forEach((c, i) => {
            const div = document.createElement("div");
            div.className = "field";
            div.style.animationDelay = `${.12 + i * .02}s`;
            div.innerHTML = `
                <div class="field-label">
                    <div style="display:flex;align-items:center;gap:4px;">
                        <span style="width:9px;height:9px;border-radius:50%;background:${c.color};display:inline-block;flex-shrink:0;"></span>
                        ${c.label}
                        <span class="tip-icon">?<span class="tip-text">${c.tip}</span></span>
                    </div>
                    <small>${c.min ? `Required — min $${c.min}/month` : c.warnPct ? `Keep under ${Math.round(c.warnPct * 100)}% of income` : "Optional"}</small>
                </div>
                <div>
                    <div class="input-wrap">
                        <span class="dollar">$</span>
                        <input type="number" id="${c.key}" min="${c.min || 0}" step="1" placeholder="0.00" aria-label="${c.label}" />
                    </div>
                    <div class="field-warn" id="warn-${c.key}"></div>
                </div>
            `;
            container.appendChild(div);
        });
    }

    // ── Build recommendation rows ──────────────────────

    function buildRecs() {
        const container = $("rec-rows");
        if (!container) return;

        CATS.forEach(c => {
            const row = document.createElement("div");
            row.className = "rec-row";
            row.id = `rec-${c.key}`;
            row.innerHTML = `
                <div class="rec-label">
                    <span class="rec-dot" style="background:${c.color}"></span>
                    ${c.label}
                </div>
                <div style="display:flex;align-items:center;gap:.5rem;">
                    <span class="rec-pct">${Math.round(c.pct * 100)}%</span>
                    <span class="rec-amt" id="rec-amt-${c.key}">—</span>
                    <span class="rec-status" id="rec-st-${c.key}"></span>
                </div>
            `;
            container.appendChild(row);
        });
    }

    // ── Donut chart ────────────────────────────────────

    function drawDonut(slices, income) {
        const svg = $("donut-svg");
        if (!svg) return;

        svg.querySelectorAll(".seg").forEach(e => e.remove());

        const total = slices.reduce((s, sl) => s + sl.val, 0);
        const totalEl = $("donut-total");
        if (totalEl) totalEl.textContent = fmtDollar(total);

        if (total === 0) return;

        const base = Math.max(income, total);
        let offset = -0.25 * CIRCUMFERENCE;

        function addArc(fraction, color) {
            if (fraction <= 0) return;
            const dash = fraction * CIRCUMFERENCE;
            const gap = CIRCUMFERENCE - dash;
            const circle = document.createElementNS("http://www.w3.org/2000/svg", "circle");
            circle.setAttribute("class", "seg");
            circle.setAttribute("cx", "80");
            circle.setAttribute("cy", "80");
            circle.setAttribute("r", "60");
            circle.setAttribute("fill", "none");
            circle.setAttribute("stroke", color);
            circle.setAttribute("stroke-width", "22");
            circle.setAttribute("stroke-dasharray", `${dash} ${gap}`);
            circle.setAttribute("stroke-dashoffset", -offset);
            const textEls = svg.querySelectorAll("text");
            svg.insertBefore(circle, textEls[0] || null);
            offset += dash;
        }

        slices.forEach(sl => { if (sl.val > 0) addArc(sl.val / base, sl.color); });

        const remaining = income - total;
        if (remaining > 1 && income > 0) addArc(remaining / base, "#e8e8e8");
    }

    // ── Legend ─────────────────────────────────────────

    function renderLegend(slices, income) {
        const lg = $("summary-rows");
        if (!lg) return;
        lg.innerHTML = "";

        const total = slices.reduce((s, sl) => s + sl.val, 0);

        slices.forEach(sl => {
            if (sl.val === 0) return;
            const pct = total > 0 ? (sl.val / Math.max(income, total) * 100).toFixed(0) : 0;
            const row = document.createElement("div");
            row.className = "summary-row";
            row.innerHTML = `
                <div class="row-label">
                    <span class="dot" style="background:${sl.color}"></span>
                    ${sl.label}
                </div>
                <div style="display:flex;align-items:center;gap:.35rem;">
                    <span style="font-size:.72rem;color:var(--muted)">${pct}%</span>
                    <span class="row-value">${fmtDollar(sl.val)}</span>
                </div>
            `;
            lg.appendChild(row);
        });
    }

    // ── Recommendations ────────────────────────────────

    function renderRecs(income) {
        CATS.forEach(c => {
            const amtEl = $(`rec-amt-${c.key}`);
            const stEl = $(`rec-st-${c.key}`);
            if (!amtEl || !stEl) return;

            const recommended = income * c.pct;
            const actual = getVal(c.key);
            amtEl.textContent = income > 0 ? fmtDollar(recommended) : "—";

            if (income > 0 && actual > 0) {
                if (c.warnPct && actual > income * c.warnPct) {
                    stEl.textContent = "HIGH";
                    stEl.className = "rec-status hi";
                } else if (actual <= income * c.pct * 1.1) {
                    stEl.textContent = "OK";
                    stEl.className = "rec-status ok";
                } else {
                    stEl.textContent = "";
                    stEl.className = "rec-status";
                }
            } else {
                stEl.textContent = "";
                stEl.className = "rec-status";
            }
        });
    }

    // ── Per-field warnings ─────────────────────────────

    function checkWarnings(income) {
        CATS.forEach(c => {
            const warnEl = $(`warn-${c.key}`);
            const inputEl = $(c.key);
            if (!warnEl || !inputEl) return;

            const val = getVal(c.key);
            inputEl.classList.remove("warn-field", "error-field");
            warnEl.textContent = "";

            if (c.min && val > 0 && val < c.min) {
                warnEl.textContent = `Minimum $${c.min}`;
                inputEl.classList.add("error-field");
            } else if (c.warnPct && income > 0 && val > income * c.warnPct) {
                warnEl.textContent = `Above ${Math.round(c.warnPct * 100)}% limit`;
                inputEl.classList.add("warn-field");
            }
        });
    }

    // ── Main calculate ─────────────────────────────────

    function calculate() {
        const income = getVal("income");

        const sumIncomeEl = $("sum-income");
        if (sumIncomeEl) sumIncomeEl.textContent = fmtDollar(income);

        const slices = CATS.map(c => ({ key: c.key, label: c.label, color: c.color, val: getVal(c.key) }));
        const total = slices.reduce((s, sl) => s + sl.val, 0);
        const diff = income - total;

        // Difference row
        const diffRow = $("difference-row");
        const diffVal = $("difference-val");
        if (diffRow && diffVal) {
            diffRow.className = "difference-row";
            if (Math.abs(diff) < 0.005) {
                diffVal.textContent = "$0.00";
                diffRow.classList.add("balanced");
            } else if (diff > 0) {
                diffVal.textContent = `+${fmtDollar(diff)} unallocated`;
                diffRow.classList.add("surplus");
            } else {
                diffVal.textContent = `-${fmtDollar(Math.abs(diff))} over`;
                diffRow.classList.add("overspent");
            }
        }

        // Alerts
        const aSurplus = $("alert-surplus");
        const aOver = $("alert-overspent");
        const aBalanced = $("alert-balanced");
        if (aSurplus) aSurplus.className = diff > 0.005 ? "alert warn visible" : "alert warn";
        if (aOver) aOver.className = diff < -0.005 ? "alert danger visible" : "alert danger";
        if (aBalanced) aBalanced.className = Math.abs(diff) < 0.005 && income > 0 && total > 0 ? "alert info visible" : "alert info";

        drawDonut(slices, income);
        renderLegend(slices, income);
        renderRecs(income);
        checkWarnings(income);
    }

    // ── Auto-fill (SOS) ────────────────────────────────

    function autoFill() {
        const income = getVal("income");
        const alertSos = $("alert-sos");
        const sosText = $("alert-sos-text");

        if (!income) {
            if (alertSos) alertSos.className = "alert danger visible";
            if (sosText) sosText.textContent = "Please enter your monthly income first.";
            setTimeout(() => { if (alertSos) alertSos.className = "alert danger"; }, 3000);
            return;
        }

        CATS.forEach(c => {
            const el = $(c.key);
            if (el) el.value = Math.round(income * c.pct);
        });

        if (alertSos) alertSos.className = "alert info visible";
        if (sosText) sosText.textContent = `Fields filled using DR's recommended percentages for ${fmtDollar(income)} income. Adjust to fit your life!`;

        calculate();
    }

    // ── Reset ──────────────────────────────────────────

    function resetAll() {
        ["income", ...CATS.map(c => c.key)].forEach(id => {
            const el = $(id);
            if (el) el.value = "";
        });

        const alertSos = $("alert-sos");
        if (alertSos) alertSos.className = "alert info";

        CATS.forEach(c => {
            const warnEl = $(`warn-${c.key}`);
            const inputEl = $(c.key);
            if (warnEl) warnEl.textContent = "";
            if (inputEl) inputEl.classList.remove("warn-field", "error-field");
        });

        calculate();
    }

    // ── Init ───────────────────────────────────────────

    function init() {
        buildFields();
        buildRecs();

        // Bind inputs
        ["income", ...CATS.map(c => c.key)].forEach(id => {
            const el = $(id);
            if (el) el.addEventListener("input", calculate);
        });

        // Buttons
        const btnSos = $("btn-sos");
        const btnReset = $("btn-reset");
        if (btnSos) btnSos.addEventListener("click", autoFill);
        if (btnReset) btnReset.addEventListener("click", resetAll);

        calculate();
    }

    document.addEventListener("DOMContentLoaded", init);

})();