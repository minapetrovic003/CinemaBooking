import { useState, useEffect, useCallback, useRef, useMemo } from "react";

const API = import.meta.env.VITE_API_BASE_URL || "http://localhost:5295";
const BELGRADE_TZ = "Europe/Belgrade";

const css = `
  @import url('https://fonts.googleapis.com/css2?family=Playfair+Display:wght@400;600;700&family=Inter:wght@300;400;500;600&display=swap');

  * { box-sizing: border-box; margin: 0; padding: 0; }

  body {
    font-family: 'Inter', sans-serif;
    background: #0e0b0b;
    color: #f5ede8;
    min-height: 100vh;
  }

  :root {
    --rose: #CC8B86;
    --cream: #F9EAE1;
    --burgundy: #7D4F50;
    --sand: #D1BE9C;
    --taupe: #AA998F;
    --dark: #0e0b0b;
    --dark2: #1a1212;
    --dark3: #261b1b;
    --text: #f5ede8;
    --text-muted: #b09a90;
  }

  .app { min-height: 100vh; }

  nav {
    position: sticky; top: 0; z-index: 100;
    background: rgba(14,11,11,0.92);
    backdrop-filter: blur(12px);
    border-bottom: 1px solid rgba(204,139,134,0.15);
    padding: 0 2rem;
    display: flex; align-items: center; justify-content: space-between;
    height: 64px;
  }
  .nav-logo {
    font-family: 'Playfair Display', serif;
    font-size: 1.5rem; font-weight: 700;
    color: var(--rose); letter-spacing: 0.02em;
    cursor: pointer;
  }
  .nav-logo span { color: var(--sand); }
  .nav-links { display: flex; gap: 0.25rem; align-items: center; }
  .nav-btn {
    background: none; border: none; color: var(--text-muted);
    font-family: 'Inter', sans-serif; font-size: 0.85rem;
    padding: 0.5rem 1rem; border-radius: 8px;
    cursor: pointer; transition: all 0.2s;
    letter-spacing: 0.03em;
  }
  .nav-btn:hover { color: var(--text); background: rgba(204,139,134,0.08); }
  .nav-btn.active { color: var(--rose); }
  .nav-auth { display: flex; gap: 0.5rem; align-items: center; }
  .btn-outline {
    background: none; border: 1px solid rgba(204,139,134,0.4);
    color: var(--rose); font-family: 'Inter', sans-serif;
    font-size: 0.85rem; padding: 0.45rem 1.1rem;
    border-radius: 8px; cursor: pointer; transition: all 0.2s;
  }
  .btn-outline:hover { background: rgba(204,139,134,0.1); }
  .btn-primary {
    background: var(--rose); border: none;
    color: #1a0e0e; font-family: 'Inter', sans-serif;
    font-size: 0.85rem; font-weight: 600;
    padding: 0.45rem 1.1rem; border-radius: 8px;
    cursor: pointer; transition: all 0.2s;
  }
  .btn-primary:hover { background: #d49a95; transform: translateY(-1px); }
  .btn-primary:disabled { opacity: 0.5; cursor: not-allowed; transform: none; }
  .btn-success {
    background: #27ae60; border: none;
    color: #fff; font-family: 'Inter', sans-serif;
    font-size: 0.85rem; font-weight: 600;
    padding: 0.45rem 1.1rem; border-radius: 8px;
    cursor: pointer; transition: all 0.2s;
  }
  .btn-success:hover { background: #2ecc71; transform: translateY(-1px); }
  .btn-success:disabled { opacity: 0.5; cursor: not-allowed; transform: none; }

  .user-badge {
    display: flex; align-items: center; gap: 0.6rem;
    background: rgba(204,139,134,0.1);
    border: 1px solid rgba(204,139,134,0.2);
    border-radius: 24px; padding: 0.3rem 0.8rem 0.3rem 0.3rem;
    cursor: pointer;
  }
  .user-avatar {
    width: 28px; height: 28px; border-radius: 50%;
    background: var(--burgundy); display: flex; align-items: center;
    justify-content: center; font-size: 11px; font-weight: 600; color: var(--cream);
  }
  .user-badge-name { font-size: 0.82rem; color: var(--text); }
  .user-badge-role {
    font-size: 0.7rem; color: var(--rose);
    background: rgba(204,139,134,0.12);
    border-radius: 4px; padding: 1px 6px;
  }

  .hero {
    position: relative; overflow: hidden;
    background: linear-gradient(160deg, #1a0f0f 0%, #0e0b0b 50%, #110d0d 100%);
    padding: 5rem 2rem 4rem;
    text-align: center;
  }
  .hero::before {
    content: '';
    position: absolute; inset: 0;
    background: radial-gradient(ellipse 80% 60% at 50% -10%, rgba(125,79,80,0.25) 0%, transparent 70%);
    pointer-events: none;
  }
  .hero-eyebrow {
    font-size: 0.75rem; letter-spacing: 0.25em;
    color: var(--rose); text-transform: uppercase;
    margin-bottom: 1.2rem;
  }
  .hero-title {
    font-family: 'Playfair Display', serif;
    font-size: clamp(2.5rem, 6vw, 4.5rem);
    font-weight: 700; line-height: 1.05;
    color: var(--cream); margin-bottom: 1rem;
  }
  .hero-title em { color: var(--rose); font-style: normal; }
  .hero-sub {
    font-size: 1rem; color: var(--text-muted);
    max-width: 480px; margin: 0 auto 2.5rem;
    line-height: 1.7;
  }

  .section { padding: 3rem 2rem; max-width: 1200px; margin: 0 auto; }
  .section-header {
    display: flex; align-items: baseline; justify-content: space-between;
    margin-bottom: 2rem;
  }
  .section-title {
    font-family: 'Playfair Display', serif;
    font-size: 1.8rem; color: var(--cream); font-weight: 600;
  }
  .section-link {
    font-size: 0.85rem; color: var(--rose);
    background: none; border: none; cursor: pointer;
  }

  .filters { display: flex; gap: 0.5rem; margin-bottom: 1.5rem; flex-wrap: wrap; }
  .filter-chip {
    background: rgba(204,139,134,0.08);
    border: 1px solid rgba(204,139,134,0.15);
    color: var(--text-muted); font-size: 0.8rem;
    padding: 0.4rem 1rem; border-radius: 20px;
    cursor: pointer; transition: all 0.2s; font-family: 'Inter', sans-serif;
  }
  .filter-chip.active, .filter-chip:hover {
    background: rgba(204,139,134,0.15);
    border-color: rgba(204,139,134,0.4);
    color: var(--rose);
  }
  .search-box {
    background: rgba(255,255,255,0.04);
    border: 1px solid rgba(255,255,255,0.08);
    border-radius: 10px; padding: 0.6rem 1rem;
    color: var(--text); font-family: 'Inter', sans-serif; font-size: 0.9rem;
    outline: none; width: 280px;
  }
  .search-box::placeholder { color: var(--text-muted); }
  .search-box:focus { border-color: rgba(204,139,134,0.4); }

  .movie-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
    gap: 1.5rem;
  }
  .movie-card {
    background: var(--dark2);
    border: 1px solid rgba(255,255,255,0.06);
    border-radius: 16px; overflow: hidden;
    cursor: pointer; transition: all 0.25s;
  }
  .movie-card:hover {
    border-color: rgba(204,139,134,0.3);
    transform: translateY(-4px);
    box-shadow: 0 16px 40px rgba(0,0,0,0.5);
  }
  .movie-poster {
    aspect-ratio: 2/3; background: var(--dark3);
    display: flex; align-items: center; justify-content: center;
    position: relative; overflow: hidden;
  }
  .movie-poster-art { width: 100%; height: 100%; display: flex; align-items: center; justify-content: center; font-size: 4rem; }
  .movie-genre-badge {
    position: absolute; top: 10px; left: 10px;
    background: rgba(0,0,0,0.7); backdrop-filter: blur(4px);
    color: var(--sand); font-size: 0.68rem; letter-spacing: 0.08em;
    text-transform: uppercase; padding: 3px 8px; border-radius: 6px;
  }
  .movie-rating {
    position: absolute; top: 10px; right: 10px;
    background: rgba(204,139,134,0.9);
    color: #1a0e0e; font-size: 0.75rem; font-weight: 700;
    padding: 3px 8px; border-radius: 6px;
  }
  .movie-info { padding: 1rem; }
  .movie-title { font-family: 'Playfair Display', serif; font-size: 1rem; font-weight: 600; color: var(--cream); margin-bottom: 0.3rem; line-height: 1.3; }
  .movie-meta { font-size: 0.78rem; color: var(--text-muted); display: flex; gap: 0.8rem; }
  .movie-showtimes-count { font-size: 0.75rem; color: var(--rose); margin-top: 0.5rem; }

  .showtime-list { display: flex; flex-direction: column; gap: 1rem; }
  .showtime-card {
    background: var(--dark2);
    border: 1px solid rgba(255,255,255,0.06);
    border-radius: 14px; padding: 1.25rem 1.5rem;
    display: flex; align-items: center; justify-content: space-between;
    gap: 1rem; cursor: pointer; transition: all 0.2s;
    flex-wrap: wrap;
  }
  .showtime-card:hover { border-color: rgba(204,139,134,0.25); background: rgba(204,139,134,0.04); }
  .showtime-movie { font-family: 'Playfair Display', serif; font-size: 1.05rem; color: var(--cream); }
  .showtime-details { display: flex; gap: 1.5rem; flex-wrap: wrap; }
  .showtime-detail { text-align: center; }
  .showtime-detail-label { font-size: 0.7rem; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.08em; }
  .showtime-detail-val { font-size: 0.95rem; color: var(--cream); font-weight: 500; margin-top: 2px; }
  .showtime-price { font-size: 1.3rem; font-weight: 700; color: var(--rose); }
  .avail-good { color: #7db87d; }
  .avail-low { color: #d4a843; }
  .avail-none { color: #cc6666; }

  .modal-overlay {
    position: fixed; inset: 0; z-index: 200;
    background: rgba(0,0,0,0.85);
    display: flex; align-items: center; justify-content: center;
    padding: 1rem;
  }
  .modal {
    background: #1a1212;
    border: 1px solid rgba(204,139,134,0.2);
    border-radius: 20px;
    max-width: 520px; width: 100%;
    max-height: 90vh; overflow-y: auto;
  }
  .modal-header {
    padding: 1.5rem 1.75rem 1rem;
    display: flex; align-items: center; justify-content: space-between;
    border-bottom: 1px solid rgba(255,255,255,0.06);
  }
  .modal-title { font-family: 'Playfair Display', serif; font-size: 1.3rem; color: var(--cream); }
  .modal-close {
    background: none; border: none; color: var(--text-muted);
    font-size: 1.5rem; cursor: pointer; line-height: 1;
    padding: 0.2rem 0.5rem; border-radius: 6px;
  }
  .modal-close:hover { color: var(--text); background: rgba(255,255,255,0.06); }
  .modal-body { padding: 1.5rem 1.75rem; }

  .form-group { margin-bottom: 1.1rem; }
  .form-label { font-size: 0.8rem; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.08em; display: block; margin-bottom: 0.4rem; }
  .form-input {
    width: 100%; background: rgba(255,255,255,0.04);
    border: 1px solid rgba(255,255,255,0.1); border-radius: 10px;
    color: var(--text); font-family: 'Inter', sans-serif; font-size: 0.9rem;
    padding: 0.65rem 1rem; outline: none; transition: border-color 0.2s;
  }
  .form-input:focus { border-color: rgba(204,139,134,0.5); }
  .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
  .form-error { font-size: 0.78rem; color: #cc6666; margin-top: 0.3rem; }
  .form-success { font-size: 0.85rem; color: #7db87d; margin-top: 0.5rem; }

  .seat-map-container { padding: 1rem; background: rgba(0,0,0,0.3); border-radius: 14px; overflow-x: auto; }
  .screen-3d {
    position: relative; margin: 0 auto 2.5rem;
    width: 90%; max-width: 400px; height: 30px;
    transform: perspective(300px) rotateX(-15deg);
    transform-origin: top center;
  }
  .screen-surface {
    position: absolute; inset: 0;
    background: linear-gradient(to bottom, rgba(204,139,134,0.7) 0%, rgba(125,79,80,0.3) 100%);
    border-radius: 4px 4px 0 0;
    display: flex; align-items: center; justify-content: center;
    font-size: 0.65rem; letter-spacing: 0.2em; text-transform: uppercase;
    color: rgba(249,234,225,0.8); font-weight: 500;
    box-shadow: 0 8px 32px rgba(204,139,134,0.2), 0 2px 0 rgba(204,139,134,0.5);
  }
  .screen-glow { position: absolute; bottom: -20px; left: 0; right: 0; height: 20px; background: radial-gradient(ellipse at center top, rgba(204,139,134,0.12) 0%, transparent 80%); }
  .seats-grid { display: flex; flex-direction: column; gap: 6px; align-items: center; }
  .seat-row { display: flex; align-items: center; gap: 5px; }
  .seat-row-label { font-size: 0.7rem; color: var(--text-muted); width: 18px; text-align: center; font-weight: 500; }
  .seat {
    width: 30px; height: 28px; border-radius: 6px 6px 4px 4px;
    cursor: pointer; position: relative; transition: all 0.15s;
    border: none; outline: none;
    display: flex; align-items: center; justify-content: center;
    font-size: 0.6rem; font-weight: 600; color: rgba(255,255,255,0.5);
    font-family: 'Inter', sans-serif;
  }
  .seat::after { content: ''; position: absolute; bottom: 0; left: 3px; right: 3px; height: 4px; border-radius: 0 0 3px 3px; background: rgba(0,0,0,0.25); }
  .seat-available { background: linear-gradient(160deg, #2d2020 0%, #231818 100%); border: 1px solid rgba(204,139,134,0.2); color: rgba(204,139,134,0.6); transform: perspective(80px) rotateX(8deg); }
  .seat-available:hover { background: linear-gradient(160deg, rgba(204,139,134,0.25) 0%, rgba(125,79,80,0.2) 100%); border-color: rgba(204,139,134,0.6); color: var(--rose); transform: perspective(80px) rotateX(8deg) translateY(-3px) scale(1.05); box-shadow: 0 6px 16px rgba(204,139,134,0.2); }
  .seat-selected { background: linear-gradient(160deg, var(--rose) 0%, var(--burgundy) 100%); border: 1px solid var(--rose); color: #1a0e0e; transform: perspective(80px) rotateX(8deg) translateY(-4px) scale(1.08); box-shadow: 0 8px 20px rgba(204,139,134,0.35); }
  .seat-booked { background: rgba(255,255,255,0.04); border: 1px solid rgba(255,255,255,0.05); color: rgba(255,255,255,0.12); cursor: not-allowed; transform: perspective(80px) rotateX(8deg); }
  .seat-locked { background: rgba(212,168,67,0.12); border: 1px solid rgba(212,168,67,0.25); color: rgba(212,168,67,0.5); cursor: not-allowed; transform: perspective(80px) rotateX(8deg); }
  .seat-mylock { background: linear-gradient(160deg, rgba(204,139,134,0.15) 0%, rgba(125,79,80,0.12) 100%); border: 1px solid rgba(204,139,134,0.4); color: var(--rose); transform: perspective(80px) rotateX(8deg) translateY(-2px); }
  .seat-vip { width: 36px; height: 32px; border-radius: 8px 8px 5px 5px; }
  .seat-legend { display: flex; gap: 1.2rem; justify-content: center; margin-top: 1.5rem; flex-wrap: wrap; }
  .legend-item { display: flex; align-items: center; gap: 5px; font-size: 0.75rem; color: var(--text-muted); }
  .legend-dot { width: 12px; height: 12px; border-radius: 3px; }

  .booking-summary { background: rgba(204,139,134,0.06); border: 1px solid rgba(204,139,134,0.15); border-radius: 12px; padding: 1.25rem; margin-top: 1.25rem; }
  .booking-summary-row { display: flex; justify-content: space-between; font-size: 0.88rem; margin-bottom: 0.5rem; }
  .booking-summary-row.total { border-top: 1px solid rgba(204,139,134,0.15); padding-top: 0.75rem; margin-top: 0.5rem; font-size: 1rem; font-weight: 600; color: var(--cream); }
  .booking-summary-val { color: var(--cream); }

  .lock-timer { display: flex; align-items: center; gap: 0.5rem; background: rgba(212,168,67,0.1); border: 1px solid rgba(212,168,67,0.2); border-radius: 8px; padding: 0.5rem 0.75rem; font-size: 0.8rem; color: #d4a843; margin-bottom: 1rem; }

  .poll-indicator { display: flex; align-items: center; gap: 0.4rem; font-size: 0.7rem; color: var(--text-muted); margin-bottom: 0.75rem; }
  .poll-dot { width: 6px; height: 6px; border-radius: 50%; background: #7db87d; animation: pulse 2s infinite; }
  @keyframes pulse { 0%, 100% { opacity: 1; } 50% { opacity: 0.3; } }

  .admin-tabs { display: flex; gap: 0.25rem; margin-bottom: 2rem; background: rgba(255,255,255,0.03); border-radius: 12px; padding: 0.25rem; overflow-x: auto; }
  .admin-tab { background: none; border: none; color: var(--text-muted); font-family: 'Inter', sans-serif; font-size: 0.85rem; padding: 0.55rem 1.1rem; border-radius: 10px; cursor: pointer; transition: all 0.2s; white-space: nowrap; }
  .admin-tab.active { background: rgba(204,139,134,0.12); color: var(--rose); }
  .admin-tab:hover:not(.active) { color: var(--text); background: rgba(255,255,255,0.04); }

  .data-table { width: 100%; border-collapse: collapse; }
  .data-table th { text-align: left; font-size: 0.7rem; text-transform: uppercase; letter-spacing: 0.1em; color: var(--text-muted); font-weight: 500; padding: 0.75rem 1rem; border-bottom: 1px solid rgba(255,255,255,0.06); }
  .data-table td { padding: 0.9rem 1rem; font-size: 0.88rem; color: var(--text); border-bottom: 1px solid rgba(255,255,255,0.04); }
  .data-table tr:hover td { background: rgba(204,139,134,0.03); }
  .status-badge { font-size: 0.72rem; padding: 3px 8px; border-radius: 6px; text-transform: uppercase; letter-spacing: 0.06em; font-weight: 500; }
  .status-confirmed { background: rgba(125,184,125,0.12); color: #7db87d; }
  .status-cancelled { background: rgba(204,102,102,0.12); color: #cc6666; }
  .status-canceled { background: rgba(204,102,102,0.12); color: #cc6666; }
  .status-pending { background: rgba(212,168,67,0.12); color: #d4a843; }
  .status-checkedin { background: rgba(125,184,125,0.12); color: #7db87d; }

  .stats-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 1rem; margin-bottom: 2rem; }
  .stat-card { background: var(--dark2); border: 1px solid rgba(255,255,255,0.06); border-radius: 14px; padding: 1.25rem; }
  .stat-label { font-size: 0.75rem; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.08em; }
  .stat-val { font-size: 2rem; font-weight: 700; color: var(--cream); margin: 0.4rem 0 0; line-height: 1; }
  .stat-accent { color: var(--rose); }

  .toast-container { position: fixed; bottom: 2rem; right: 2rem; z-index: 1000; display: flex; flex-direction: column; gap: 0.5rem; }
  .toast { background: var(--dark3); border-radius: 10px; padding: 0.85rem 1.25rem; border-left: 3px solid var(--rose); font-size: 0.87rem; max-width: 320px; box-shadow: 0 8px 24px rgba(0,0,0,0.5); animation: slideIn 0.25s ease; }
  .toast.success { border-left-color: #7db87d; }
  .toast.error { border-left-color: #cc6666; }
  @keyframes slideIn { from { transform: translateX(100%); opacity: 0; } to { transform: translateX(0); opacity: 1; } }

  .booking-card { background: var(--dark2); border: 1px solid rgba(255,255,255,0.06); border-radius: 14px; padding: 1.25rem 1.5rem; margin-bottom: 1rem; transition: all 0.2s; cursor: pointer; }
  .booking-card:hover { border-color: rgba(204,139,134,0.2); }
  .booking-card-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 1rem; }
  .booking-movie-title { font-family: 'Playfair Display', serif; font-size: 1.1rem; color: var(--cream); }
  .booking-seats { display: flex; gap: 0.4rem; flex-wrap: wrap; margin-top: 0.7rem; }
  .seat-tag { background: rgba(204,139,134,0.1); border: 1px solid rgba(204,139,134,0.2); color: var(--rose); font-size: 0.75rem; padding: 2px 8px; border-radius: 6px; }
  .btn-cancel { background: none; border: 1px solid rgba(204,102,102,0.3); color: #cc6666; font-family: 'Inter', sans-serif; font-size: 0.8rem; padding: 0.35rem 0.8rem; border-radius: 8px; cursor: pointer; transition: all 0.2s; }
  .btn-cancel:hover { background: rgba(204,102,102,0.1); }

  .movie-detail-header { display: flex; gap: 2rem; margin-bottom: 2rem; flex-wrap: wrap; }
  .movie-detail-poster { width: 200px; min-width: 140px; aspect-ratio: 2/3; border-radius: 14px; background: var(--dark3); flex-shrink: 0; display: flex; align-items: center; justify-content: center; font-size: 5rem; }
  .movie-detail-info { flex: 1; }
  .movie-detail-title { font-family: 'Playfair Display', serif; font-size: 2.2rem; font-weight: 700; color: var(--cream); margin-bottom: 0.75rem; }
  .movie-detail-meta { display: flex; gap: 1rem; flex-wrap: wrap; margin-bottom: 1rem; }
  .meta-pill { background: rgba(255,255,255,0.05); border: 1px solid rgba(255,255,255,0.08); border-radius: 20px; padding: 4px 12px; font-size: 0.8rem; color: var(--text-muted); }
  .meta-pill.highlight { border-color: rgba(204,139,134,0.3); color: var(--rose); background: rgba(204,139,134,0.08); }
  .movie-detail-desc { font-size: 0.95rem; color: var(--text-muted); line-height: 1.7; margin-bottom: 1.5rem; }

  .divider { height: 1px; background: rgba(255,255,255,0.06); margin: 2rem 0; }
  .empty-state { text-align: center; padding: 4rem 1rem; color: var(--text-muted); }
  .empty-state-icon { font-size: 3rem; margin-bottom: 1rem; opacity: 0.4; }
  .spinner { width: 32px; height: 32px; border-radius: 50%; border: 2px solid rgba(204,139,134,0.2); border-top-color: var(--rose); animation: spin 0.7s linear infinite; margin: 2rem auto; }
  @keyframes spin { to { transform: rotate(360deg); } }

  .add-panel { background: var(--dark2); border: 1px solid rgba(204,139,134,0.15); border-radius: 16px; padding: 1.5rem; margin-bottom: 2rem; }
  .add-panel-title { font-family: 'Playfair Display', serif; font-size: 1.2rem; color: var(--cream); margin-bottom: 1.25rem; }

  .verify-page { min-height: 100vh; display: flex; align-items: center; justify-content: center; padding: 2rem; background: #0e0b0b; }
  .verify-card { background: var(--dark2); border: 1px solid rgba(204,139,134,0.2); border-radius: 20px; padding: 2.5rem; max-width: 480px; width: 100%; text-align: center; }
  .verify-status-icon { font-size: 4rem; margin-bottom: 1rem; }
  .verify-title { font-family: 'Playfair Display', serif; font-size: 1.6rem; color: var(--cream); margin-bottom: 0.5rem; }
  .verify-info-row { display: flex; justify-content: space-between; padding: 0.6rem 0; border-bottom: 1px solid rgba(255,255,255,0.06); font-size: 0.88rem; }
  .verify-info-label { color: var(--text-muted); }
  .verify-info-val { color: var(--cream); font-weight: 500; }
  .btn-checkin { background: #27ae60; border: none; color: #fff; font-family: 'Inter', sans-serif; font-size: 0.95rem; font-weight: 600; padding: 0.75rem 2rem; border-radius: 10px; cursor: pointer; transition: all 0.2s; margin-top: 1.5rem; width: 100%; }
  .btn-checkin:hover { background: #2ecc71; transform: translateY(-1px); }
  .btn-checkin:disabled { opacity: 0.5; cursor: not-allowed; transform: none; }

  .method-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; margin: 1rem 0; }
  .method-btn {
    background: rgba(255,255,255,0.04); border: 1px solid rgba(255,255,255,0.1);
    border-radius: 10px; padding: 0.75rem; cursor: pointer; transition: all 0.2s;
    color: var(--text-muted); font-family: 'Inter', sans-serif; font-size: 0.85rem;
    text-align: center;
  }
  .method-btn:hover, .method-btn.selected {
    border-color: rgba(204,139,134,0.5); background: rgba(204,139,134,0.08); color: var(--rose);
  }

  .info-box { background: rgba(204,139,134,0.06); border: 1px solid rgba(204,139,134,0.15); border-radius: 10px; padding: 0.85rem 1rem; font-size: 0.83rem; color: var(--text-muted); margin-top: 0.75rem; }
`;

const GENRE_EMOJI = {
  "Action": "⚔️", "Comedy": "😂", "Drama": "🎭", "Horror": "👻",
  "Thriller": "🔪", "Romance": "💕", "Sci-Fi": "🚀", "Animation": "🎨",
  "Adventure": "🗺️", "Mystery": "🔍", "Fantasy": "🧙", "Crime": "🔫",
};
const getEmoji = (genre) => GENRE_EMOJI[genre] || "🎬";
const GENRES = ["All", "Action", "Comedy", "Drama", "Horror", "Thriller", "Romance", "Sci-Fi", "Animation", "Adventure", "Mystery", "Fantasy", "Crime"];
const PAYMENT_METHODS = ["CreditCard", "DebitCard", "PayPal", "Voucher"];

function formatEur(amount) { return `€${Number(amount).toFixed(2)}`; }

function fmtDate(dt, opts = {}) {
  return new Date(dt).toLocaleDateString("en-GB", { timeZone: BELGRADE_TZ, ...opts });
}
function fmtTime(dt) {
  return new Date(dt).toLocaleTimeString("en-GB", { timeZone: BELGRADE_TZ, hour: "2-digit", minute: "2-digit" });
}
function fmtDateTime(dt) {
  return new Date(dt).toLocaleString("en-GB", { timeZone: BELGRADE_TZ, day: "numeric", month: "short", hour: "2-digit", minute: "2-digit" });
}

// Mapa statusa za konzistentni prikaz — CheckedIn se prikazuje kao Confirmed
function displayStatus(status) {
  if (!status) return "Confirmed";
  if (status === "CheckedIn") return "Confirmed ✓";
  return status;
}
function statusClass(status) {
  if (!status) return "confirmed";
  if (status === "CheckedIn") return "confirmed";
  return status.toLowerCase();
}

function useToast() {
  const [toasts, setToasts] = useState([]);
  const add = useCallback((msg, type = "info") => {
    const id = Date.now();
    setToasts(t => [...t, { id, msg, type }]);
    setTimeout(() => setToasts(t => t.filter(x => x.id !== id)), 4000);
  }, []);
  return { toasts, add };
}

function parseJwt(token) {
  try { return JSON.parse(atob(token.split(".")[1])); } catch { return null; }
}

function generateUUID() {
  return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, (c) => {
    const r = (Math.random() * 16) | 0;
    return (c === "x" ? r : (r & 0x3) | 0x8).toString(16);
  });
}

async function safeJson(res) {
  const text = await res.text();
  if (!text) return {};
  try { return JSON.parse(text); } catch { return { message: text }; }
}

async function authFetch(url, token, options = {}) {
  const method = (options.method || "GET").toUpperCase();
  const headers = {
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...(method === "POST" ? { "Idempotency-Key": generateUUID() } : {}),
    ...(options.headers || {}),
  };
  return fetch(url, { ...options, headers });
}

function parseVerifyHash() {
  const hash = window.location.hash;
  const match = hash.match(/^#verify\/(\d+)$/);
  return match ? parseInt(match[1], 10) : null;
}

export default function App() {
  const [page, setPage] = useState("home");
  const [token, setToken] = useState(() => localStorage.getItem("cb_token") || null);
  const [authModal, setAuthModal] = useState(null);
  const [verifyBookingId, setVerifyBookingId] = useState(() => parseVerifyHash());
  const { toasts, add: toast } = useToast();

  const user = useMemo(() => {
    if (!token) return null;
    const p = parseJwt(token);
    if (!p) return null;
    const role = p["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || p.role || "User";
    const email = p["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] || p.email || "";
    const name = p["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || p.name || email;
    return { email, name, role, token };
  }, [token]);

  useEffect(() => {
    const onHashChange = () => setVerifyBookingId(parseVerifyHash());
    window.addEventListener("hashchange", onHashChange);
    return () => window.removeEventListener("hashchange", onHashChange);
  }, []);

  const logout = () => { localStorage.removeItem("cb_token"); setToken(null); setPage("home"); toast("Logged out", "info"); };
  const handleLogin = (tkn) => { localStorage.setItem("cb_token", tkn); setToken(tkn); setAuthModal(null); toast("Welcome back!", "success"); };
  const isAdmin = user?.role === "Admin";

  if (verifyBookingId) {
    return (
      <>
        <style>{css}</style>
        <VerifyPage bookingId={verifyBookingId} token={token} toast={toast}
          onBack={() => { window.location.hash = ""; setVerifyBookingId(null); }} />
        <div className="toast-container">{toasts.map(t => <div key={t.id} className={`toast ${t.type}`}>{t.msg}</div>)}</div>
      </>
    );
  }

  return (
    <>
      <style>{css}</style>
      <div className="app">
        <nav>
          <div className="nav-logo" onClick={() => setPage("home")}>Cinema<span>Verse</span></div>
          <div className="nav-links">
            <button className={`nav-btn ${page === "home" ? "active" : ""}`} onClick={() => setPage("home")}>Movies</button>
            <button className={`nav-btn ${page === "showtimes" ? "active" : ""}`} onClick={() => setPage("showtimes")}>Showtimes</button>
            {user && <button className={`nav-btn ${page === "mybookings" ? "active" : ""}`} onClick={() => setPage("mybookings")}>My Bookings</button>}
            {isAdmin && <button className={`nav-btn ${page === "admin" ? "active" : ""}`} onClick={() => setPage("admin")}>Admin</button>}
          </div>
          <div className="nav-auth">
            {user ? (
              <div className="user-badge" onClick={logout} title="Click to logout">
                <div className="user-avatar">{user.name?.[0]?.toUpperCase() || "U"}</div>
                <span className="user-badge-name">{user.name?.split("@")[0]}</span>
                <span className="user-badge-role">{user.role}</span>
              </div>
            ) : (
              <>
                <button className="btn-outline" onClick={() => setAuthModal("login")}>Login</button>
                <button className="btn-primary" onClick={() => setAuthModal("register")}>Register</button>
              </>
            )}
          </div>
        </nav>

        {page === "home" && <MoviesPage setPage={setPage} user={user} toast={toast} setAuthModal={setAuthModal} />}
        {page === "showtimes" && <ShowtimesPage user={user} token={token} toast={toast} setAuthModal={setAuthModal} />}
        {page === "mybookings" && user && <MyBookingsPage user={user} token={token} toast={toast} />}
        {page === "admin" && isAdmin && <AdminPage token={token} toast={toast} />}

        {authModal && (
          <AuthModal mode={authModal} onClose={() => setAuthModal(null)} onLogin={handleLogin}
            switchMode={(m) => setAuthModal(m)} toast={toast} />
        )}
        <div className="toast-container">{toasts.map(t => <div key={t.id} className={`toast ${t.type}`}>{t.msg}</div>)}</div>
      </div>
    </>
  );
}

// ─── VERIFY PAGE ───
function VerifyPage({ bookingId, token, toast, onBack }) {
  const [booking, setBooking] = useState(null);
  const [loading, setLoading] = useState(true);
  const [checkingIn, setCheckingIn] = useState(false);
  const [done, setDone] = useState(false);

  useEffect(() => {
    fetch(`${API}/bookings/${bookingId}/verify`)
      .then(r => r.json())
      .then(d => { setBooking(d); setLoading(false); })
      .catch(() => { setLoading(false); toast("Could not load booking.", "error"); });
  }, [bookingId]);

  const handleCheckIn = async () => {
    setCheckingIn(true);
    try {
      const r = await authFetch(`${API}/bookings/${bookingId}/checkin`, token, { method: "PATCH" });
      if (r.ok || r.status === 204) {
        setDone(true);
        setBooking(b => b ? { ...b, status: "CheckedIn" } : b);
        toast("Checked in successfully!", "success");
      } else {
        const e = await safeJson(r);
        toast(e.message || "Check-in failed.", "error");
      }
    } catch { toast("Connection error.", "error"); }
    setCheckingIn(false);
  };

  const currentStatus = booking?.status;
  const isConfirmed = currentStatus === "Confirmed";
  const isCheckedIn = currentStatus === "CheckedIn" || done;
  const isCanceled = currentStatus === "Cancelled" || currentStatus === "Canceled";

  return (
    <div className="verify-page">
      <div className="verify-card">
        <button className="btn-outline" style={{ marginBottom: "1.5rem" }} onClick={onBack}>← Back to App</button>
        {loading ? <div className="spinner" /> : !booking ? (
          <div>
            <div className="verify-status-icon">❓</div>
            <div className="verify-title">Booking Not Found</div>
            <p style={{ color: "var(--text-muted)", marginTop: "0.5rem" }}>No booking found with ID #{bookingId}.</p>
          </div>
        ) : (
          <>
            <div className="verify-status-icon">{isCheckedIn ? "✅" : isConfirmed ? "🎬" : isCanceled ? "❌" : "🎟"}</div>
            <div className="verify-title">Booking #{booking.bookingId}</div>
            <div style={{ marginBottom: "0.5rem" }}>
              <span className={`status-badge status-${statusClass(currentStatus)}`}>
                {displayStatus(currentStatus)}
              </span>
            </div>
            <div style={{ margin: "1.5rem 0", textAlign: "left" }}>
              {[
                ["Guest", booking.customerName],
                ["Movie", booking.movie],
                ["Hall", booking.hall],
                ["Date & Time", fmtDateTime(booking.showtime)],
                ["Seats", Array.isArray(booking.seats) ? booking.seats.join(", ") : booking.seats],
                ["Total", formatEur(booking.totalPrice)],
              ].map(([label, val]) => (
                <div key={label} className="verify-info-row">
                  <span className="verify-info-label">{label}</span>
                  <span className="verify-info-val">{val}</span>
                </div>
              ))}
            </div>

            {/* Check-in je dostupan svima — ruta je otvorena jer QR dobija samo vlasnik */}
            {isConfirmed && !done && (
              <button className="btn-checkin" onClick={handleCheckIn} disabled={checkingIn}>
                {checkingIn ? "Checking in..." : "✓ Check In"}
              </button>
            )}

            {isCheckedIn && (
              <div style={{ marginTop: "1.5rem", padding: "1rem", background: "rgba(39,174,96,0.1)", border: "1px solid rgba(39,174,96,0.3)", borderRadius: "10px", color: "#27ae60" }}>
                ✅ Successfully checked in
              </div>
            )}

            {!isConfirmed && !isCheckedIn && (
              <div style={{ marginTop: "1.5rem", padding: "1rem", background: "rgba(204,102,102,0.1)", border: "1px solid rgba(204,102,102,0.3)", borderRadius: "10px", color: "#cc6666" }}>
                ⚠️ Booking cannot be checked in (status: {currentStatus})
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
}

// ─── MOVIES PAGE ───
function MoviesPage({ setPage, user, toast, setAuthModal }) {
  const [movies, setMovies] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [genre, setGenre] = useState("All");
  const [selected, setSelected] = useState(null);

  useEffect(() => {
    fetch(`${API}/movies?pageSize=50`)
      .then(r => r.json())
      .then(d => { setMovies(d.items || d); setLoading(false); })
      .catch(() => { setLoading(false); toast("Could not connect to backend", "error"); });
  }, []);

  const filtered = movies.filter(m =>
    (genre === "All" || m.genre === genre) &&
    m.title.toLowerCase().includes(search.toLowerCase())
  );

  if (selected) return (
    <MovieDetail movie={selected} onBack={() => setSelected(null)} user={user} setAuthModal={setAuthModal} toast={toast} />
  );

  return (
    <>
      <div style={{ background: "linear-gradient(180deg, #1a0f0f 0%, #0e0b0b 100%)" }}>
        <div style={{ padding: "3.5rem 2rem 2.5rem", maxWidth: 1200, margin: "0 auto" }}>
          <p className="hero-eyebrow">Now Playing</p>
          <h1 className="hero-title" style={{ textAlign: "left", marginBottom: "1.5rem" }}>
            Your next<br /><em>cinematic journey</em>
          </h1>
          <input className="search-box" placeholder="Search movies..." value={search} onChange={e => setSearch(e.target.value)} />
          <div className="filters" style={{ marginTop: "1rem" }}>
            {GENRES.map(g => (
              <button key={g} className={`filter-chip ${genre === g ? "active" : ""}`} onClick={() => setGenre(g)}>{g}</button>
            ))}
          </div>
        </div>
      </div>
      <div className="section">
        {loading ? <div className="spinner" /> : filtered.length === 0 ? (
          <div className="empty-state"><div className="empty-state-icon">🎬</div><p>No movies found</p></div>
        ) : (
          <div className="movie-grid">
            {filtered.map(m => (
              <div key={m.id} className="movie-card" onClick={() => setSelected(m)}>
                <div className="movie-poster">
                  <div className="movie-poster-art">{getEmoji(m.genre)}</div>
                  <div className="movie-genre-badge">{m.genre}</div>
                  <div className="movie-rating">★ {Number(m.rating).toFixed(1)}</div>
                </div>
                <div className="movie-info">
                  <div className="movie-title">{m.title}</div>
                  <div className="movie-meta"><span>{m.durationMinutes} min</span><span>{m.genre}</span></div>
                  {m.showtimeCount > 0 && <div className="movie-showtimes-count">{m.showtimeCount} showtime{m.showtimeCount !== 1 ? "s" : ""}</div>}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </>
  );
}

// ─── MOVIE DETAIL ───
function MovieDetail({ movie, onBack, user, setAuthModal, toast }) {
  const [showtimes, setShowtimes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [bookingShowtime, setBookingShowtime] = useState(null);
  const [detailShowtime, setDetailShowtime] = useState(null);

  useEffect(() => {
    fetch(`${API}/showtimes?movieTitle=${encodeURIComponent(movie.title)}`)
      .then(r => r.json())
      .then(d => { setShowtimes(Array.isArray(d) ? d : d.items || []); setLoading(false); })
      .catch(() => setLoading(false));
  }, [movie.id]);

  const upcoming = showtimes.filter(s => new Date(s.startTime) > new Date());

  return (
    <div className="section">
      <button className="btn-outline" style={{ marginBottom: "1.5rem" }} onClick={onBack}>← Back to Movies</button>
      <div className="movie-detail-header">
        <div className="movie-detail-poster">{getEmoji(movie.genre)}</div>
        <div className="movie-detail-info">
          <h1 className="movie-detail-title">{movie.title}</h1>
          <div className="movie-detail-meta">
            <span className="meta-pill highlight">★ {Number(movie.rating).toFixed(1)}</span>
            <span className="meta-pill">{movie.durationMinutes} min</span>
            <span className="meta-pill">{movie.genre}</span>
          </div>
          <p className="movie-detail-desc">{movie.description || "An unforgettable cinematic experience awaits."}</p>
          {!user && <button className="btn-primary" style={{ padding: "0.65rem 1.5rem" }} onClick={() => setAuthModal("login")}>Login to Book</button>}
        </div>
      </div>
      <div className="divider" />
      <h2 style={{ fontFamily: "'Playfair Display', serif", fontSize: "1.4rem", color: "var(--cream)", marginBottom: "1.25rem" }}>Upcoming Showtimes</h2>
      {loading ? <div className="spinner" /> : upcoming.length === 0 ? (
        <div className="empty-state"><p>No upcoming showtimes for this movie</p></div>
      ) : (
        <div className="showtime-list">
          {upcoming.map(s => (
            <div key={s.id} className="showtime-card">
              <div onClick={() => setDetailShowtime(s)} style={{ flex: 1 }}>
                <div style={{ fontSize: "1rem", color: "var(--cream)", fontWeight: 500 }}>
                  {fmtDate(s.startTime, { weekday: "short", day: "numeric", month: "short" })}
                </div>
                <div style={{ fontSize: "0.85rem", color: "var(--text-muted)", marginTop: "2px" }}>
                  {fmtTime(s.startTime)} — {fmtTime(s.endTime)}
                </div>
              </div>
              <div className="showtime-details">
                <div className="showtime-detail"><div className="showtime-detail-label">Hall</div><div className="showtime-detail-val">{s.hallName}</div></div>
                <div className="showtime-detail"><div className="showtime-detail-label">Capacity</div><div className="showtime-detail-val">{s.hallCapacity}</div></div>
                <div className="showtime-detail">
                  <div className="showtime-detail-label">Available</div>
                  <div className={`showtime-detail-val ${s.availableSeats > 20 ? "avail-good" : s.availableSeats > 5 ? "avail-low" : "avail-none"}`}>{s.availableSeats}</div>
                </div>
              </div>
              <div>
                <div className="showtime-price">{formatEur(s.price)}</div>
                <button className="btn-primary" style={{ fontSize: "0.8rem", padding: "0.35rem 0.9rem", marginTop: "0.4rem" }}
                  onClick={() => user ? setBookingShowtime(s) : setAuthModal("login")}>
                  {user ? "Book now" : "Login to book"}
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
      {bookingShowtime && (
        <BookingModal showtime={bookingShowtime} movie={movie} user={user} token={user?.token}
          onClose={() => setBookingShowtime(null)} toast={toast} />
      )}
      {detailShowtime && (
        <ShowtimeDetailModal showtime={detailShowtime} movie={movie} user={user}
          onClose={() => setDetailShowtime(null)}
          onBook={(s) => { setDetailShowtime(null); if (user) setBookingShowtime(s); else setAuthModal("login"); }} />
      )}
    </div>
  );
}

// ─── SHOWTIME DETAIL MODAL ───
function ShowtimeDetailModal({ showtime, movie, user, onClose, onBook }) {
  return (
    <div className="modal-overlay" onClick={e => e.target === e.currentTarget && onClose()}>
      <div className="modal">
        <div className="modal-header">
          <div className="modal-title">Showtime Details</div>
          <button className="modal-close" onClick={onClose}>×</button>
        </div>
        <div className="modal-body">
          <div style={{ fontFamily: "'Playfair Display', serif", fontSize: "1.3rem", color: "var(--cream)", marginBottom: "1.25rem" }}>{movie?.title}</div>
          {[
            ["Date", fmtDate(showtime.startTime, { weekday: "long", day: "numeric", month: "long", year: "numeric" })],
            ["Start Time", fmtTime(showtime.startTime)],
            ["End Time", fmtTime(showtime.endTime)],
            ["Hall", showtime.hallName],
            ["Capacity", showtime.hallCapacity],
            ["Available Seats", showtime.availableSeats],
            ["Price per Seat", formatEur(showtime.price)],
          ].map(([k, v]) => (
            <div key={k} className="verify-info-row">
              <span className="verify-info-label">{k}</span>
              <span className="verify-info-val" className={k === "Available Seats" ? (showtime.availableSeats > 20 ? "avail-good" : showtime.availableSeats > 5 ? "avail-low" : "avail-none") : ""}>{v}</span>
            </div>
          ))}
          <button className="btn-primary" style={{ width: "100%", marginTop: "1.5rem", padding: "0.75rem" }}
            onClick={() => onBook(showtime)}>
            {user ? "Book This Showtime" : "Login to Book"}
          </button>
        </div>
      </div>
    </div>
  );
}

// ─── SHOWTIMES PAGE ───
function ShowtimesPage({ user, token, toast, setAuthModal }) {
  const [showtimes, setShowtimes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [bookingShowtime, setBookingShowtime] = useState(null);
  const [detailShowtime, setDetailShowtime] = useState(null);
  const [search, setSearch] = useState("");

  useEffect(() => {
    fetch(`${API}/showtimes`)
      .then(r => r.json())
      .then(d => { setShowtimes(Array.isArray(d) ? d : d.items || []); setLoading(false); })
      .catch(() => setLoading(false));
  }, []);

  const upcoming = showtimes.filter(s =>
    new Date(s.startTime) > new Date() &&
    (s.movieTitle?.toLowerCase().includes(search.toLowerCase()) || !search)
  );

  return (
    <div className="section">
      <div className="section-header">
        <h1 className="section-title">Showtimes</h1>
        <input className="search-box" style={{ width: 220 }} placeholder="Search movie..." value={search} onChange={e => setSearch(e.target.value)} />
      </div>
      {loading ? <div className="spinner" /> : upcoming.length === 0 ? (
        <div className="empty-state"><div className="empty-state-icon">🎬</div><p>No upcoming showtimes</p></div>
      ) : (
        <div className="showtime-list">
          {upcoming.map(s => (
            <div key={s.id} className="showtime-card">
              <div onClick={() => setDetailShowtime(s)} style={{ flex: 1, cursor: "pointer" }}>
                <div className="showtime-movie">{s.movieTitle}</div>
                <div style={{ fontSize: "0.8rem", color: "var(--text-muted)", marginTop: "3px" }}>{s.movieGenre}</div>
              </div>
              <div className="showtime-details">
                <div className="showtime-detail"><div className="showtime-detail-label">Date</div><div className="showtime-detail-val">{fmtDate(s.startTime, { day: "numeric", month: "short" })}</div></div>
                <div className="showtime-detail"><div className="showtime-detail-label">Time</div><div className="showtime-detail-val">{fmtTime(s.startTime)}</div></div>
                <div className="showtime-detail"><div className="showtime-detail-label">Hall</div><div className="showtime-detail-val">{s.hallName}</div></div>
                <div className="showtime-detail">
                  <div className="showtime-detail-label">Seats left</div>
                  <div className={`showtime-detail-val ${s.availableSeats > 20 ? "avail-good" : s.availableSeats > 5 ? "avail-low" : "avail-none"}`}>{s.availableSeats}</div>
                </div>
              </div>
              <div>
                <div className="showtime-price">{formatEur(s.price)}</div>
                <button className="btn-primary" style={{ fontSize: "0.8rem", padding: "0.35rem 0.9rem", marginTop: "0.4rem" }}
                  onClick={() => user ? setBookingShowtime(s) : setAuthModal("login")}>
                  {user ? "Book →" : "Login"}
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
      {bookingShowtime && (
        <BookingModal showtime={bookingShowtime}
          movie={{ title: bookingShowtime.movieTitle, genre: bookingShowtime.movieGenre }}
          user={user} token={token}
          onClose={() => setBookingShowtime(null)} toast={toast} />
      )}
      {detailShowtime && (
        <ShowtimeDetailModal showtime={detailShowtime}
          movie={{ title: detailShowtime.movieTitle }}
          user={user}
          onClose={() => setDetailShowtime(null)}
          onBook={(s) => { setDetailShowtime(null); if (user) setBookingShowtime(s); else setAuthModal("login"); }} />
      )}
    </div>
  );
}

// ─── BOOKING MODAL ───
function BookingModal({ showtime, movie, user, token, onClose, toast }) {
  const [seatMap, setSeatMap] = useState([]);
  const [selectedSeats, setSelectedSeats] = useState([]);
  const [step, setStep] = useState("seats");
  const [timerSec, setTimerSec] = useState(0);
  const [loading, setLoading] = useState(false);
  const [loadingSeats, setLoadingSeats] = useState(true);
  const [createdBooking, setCreatedBooking] = useState(null);
  const timerRef = useRef(null);
  const pollRef = useRef(null);

  const fetchSeatAvailability = useCallback(async () => {
    try {
      const res = await authFetch(`${API}/seat-locks/availability/${showtime.id}`, token);
      if (res.ok) { const data = await res.json(); setSeatMap(data); }
    } catch { }
    finally { setLoadingSeats(false); }
  }, [showtime.id, token]);

  useEffect(() => {
    fetchSeatAvailability();
    pollRef.current = setInterval(fetchSeatAvailability, 5000);
    return () => { clearInterval(pollRef.current); clearInterval(timerRef.current); };
  }, [fetchSeatAvailability]);

  useEffect(() => {
    if (step !== "seats") clearInterval(pollRef.current);
  }, [step]);

  const toggleSeat = (seat) => {
    if (seat.status === "Booked" || seat.status === "Locked") return;
    setSelectedSeats(prev => prev.includes(seat.label) ? prev.filter(s => s !== seat.label) : [...prev, seat.label]);
  };

  const lockSeats = async () => {
    if (selectedSeats.length === 0) return toast("Select at least one seat", "error");
    setLoading(true);
    try {
      const res = await authFetch(`${API}/seat-locks/lock`, token, {
        method: "POST",
        body: JSON.stringify({ userEmail: user.email, movieTitle: movie.title, hallName: showtime.hallName, showtimeStartTime: showtime.startTime, seats: selectedSeats, lockMinutes: 10 })
      });
      if (res.ok) {
        const data = await res.json();
        const secs = data.expiresInSeconds ?? Math.max(0, Math.round((new Date(data.expiresAt) - Date.now()) / 1000));
        setTimerSec(secs);
        timerRef.current = setInterval(() => setTimerSec(s => { if (s <= 1) { clearInterval(timerRef.current); return 0; } return s - 1; }), 1000);
        setStep("confirm");
      } else {
        const err = await safeJson(res);
        if (res.status === 401) toast("Session expired — please log in again.", "error");
        else if (res.status === 409) { toast((err.message || "Seats unavailable") + " — map refreshed.", "error"); await fetchSeatAvailability(); setSelectedSeats([]); }
        else toast(err.message || "Could not lock seats", "error");
      }
    } catch { toast("Network error.", "error"); }
    setLoading(false);
  };

  const confirmBooking = async () => {
    setLoading(true);
    try {
      const res = await authFetch(`${API}/bookings`, token, {
        method: "POST",
        body: JSON.stringify({ userEmail: user.email, movieTitle: movie.title, hallName: showtime.hallName, showtimeStartTime: showtime.startTime, seats: selectedSeats })
      });
      if (res.ok || res.status === 201) {
        clearInterval(timerRef.current);
        const data = await safeJson(res);
        setCreatedBooking(data);
        setStep("success");
      } else {
        const err = await safeJson(res);
        if (res.status === 401) toast("Session expired — please log in again.", "error");
        else if (res.status === 409) { toast((err.message || "Conflict") + " — re-select seats.", "error"); setStep("seats"); await fetchSeatAvailability(); setSelectedSeats([]); pollRef.current = setInterval(fetchSeatAvailability, 5000); }
        else toast(err.message || "Booking failed", "error");
      }
    } catch { toast("Network error.", "error"); }
    setLoading(false);
  };

  const releaseLocks = useCallback(() => {
    if (user?.email && showtime?.id) {
      authFetch(`${API}/seat-locks/release?userEmail=${encodeURIComponent(user.email)}&showtimeId=${showtime.id}`, token, { method: "DELETE" }).catch(() => {});
    }
  }, [user, showtime, token]);

  const handleClose = () => { if (step === "confirm") releaseLocks(); clearInterval(timerRef.current); clearInterval(pollRef.current); onClose(); };
  const handleBack = () => { releaseLocks(); clearInterval(timerRef.current); setStep("seats"); setSelectedSeats([]); pollRef.current = setInterval(fetchSeatAvailability, 5000); fetchSeatAvailability(); };

  const rows = (() => {
    const rowMap = {};
    seatMap.forEach(seat => { if (!rowMap[seat.row]) rowMap[seat.row] = []; rowMap[seat.row].push(seat); });
    return Object.entries(rowMap).sort(([a], [b]) => a.localeCompare(b)).map(([rowLabel, seats]) => ({ label: rowLabel, seats: seats.sort((a, b) => a.number - b.number) }));
  })();

  const totalPrice = selectedSeats.length * Number(showtime.price);

  return (
    <div className="modal-overlay" onClick={e => e.target === e.currentTarget && handleClose()}>
      <div className="modal" style={{ maxWidth: step === "seats" ? 680 : 480 }}>
        <div className="modal-header">
          <div className="modal-title">
            {step === "seats" && "Choose Your Seats"}
            {step === "confirm" && "Confirm Booking"}
            {step === "success" && "Booking Created"}
          </div>
          <button className="modal-close" onClick={handleClose}>×</button>
        </div>
        <div className="modal-body">
          {step === "seats" && (
            <>
              <div style={{ fontSize: "0.85rem", color: "var(--text-muted)", marginBottom: "1.25rem" }}>
                <strong style={{ color: "var(--cream)" }}>{movie.title}</strong>{" · "}{showtime.hallName}{" · "}{fmtDate(showtime.startTime, { day: "numeric", month: "short" })} {fmtTime(showtime.startTime)}
              </div>
              <div className="poll-indicator"><div className="poll-dot" /><span>Live availability — updates every 5 seconds</span></div>
              {loadingSeats ? <div className="spinner" /> : rows.length === 0 ? (
                <div className="empty-state"><p>Could not load seat map.</p></div>
              ) : (
                <div className="seat-map-container">
                  <div className="screen-3d"><div className="screen-surface">SCREEN</div><div className="screen-glow" /></div>
                  <div className="seats-grid">
                    {rows.map(row => (
                      <div key={row.label} className="seat-row">
                        <span className="seat-row-label">{row.label}</span>
                        {row.seats.map(seat => {
                          const isSelected = selectedSeats.includes(seat.label);
                          const seatClass = seat.status === "Booked" ? "seat-booked" : seat.status === "Locked" ? "seat-locked" : seat.status === "MyLock" ? "seat-mylock" : isSelected ? "seat-selected" : "seat-available";
                          return (
                            <button key={seat.label} className={`seat ${seat.seatType === "VIP" ? "seat-vip" : ""} ${seatClass}`}
                              onClick={() => toggleSeat(seat)}
                              title={`${seat.label}${seat.seatType === "VIP" ? " (VIP)" : ""} — ${seat.status}`}
                              disabled={seat.status === "Booked" || seat.status === "Locked"}>
                              {seat.number}
                            </button>
                          );
                        })}
                        <span className="seat-row-label">{row.label}</span>
                      </div>
                    ))}
                  </div>
                  <div className="seat-legend">
                    <div className="legend-item"><div className="legend-dot" style={{ background: "#2d2020", border: "1px solid rgba(204,139,134,0.3)" }} />Available</div>
                    <div className="legend-item"><div className="legend-dot" style={{ background: "var(--rose)" }} />Selected</div>
                    <div className="legend-item"><div className="legend-dot" style={{ background: "rgba(255,255,255,0.05)" }} />Taken</div>
                    <div className="legend-item"><div className="legend-dot" style={{ background: "rgba(212,168,67,0.2)", border: "1px solid rgba(212,168,67,0.4)" }} />Locked</div>
                  </div>
                </div>
              )}
              {selectedSeats.length > 0 && (
                <div className="booking-summary">
                  <div className="booking-summary-row"><span style={{ color: "var(--text-muted)" }}>Selected seats</span><span className="booking-summary-val">{selectedSeats.join(", ")}</span></div>
                  <div className="booking-summary-row"><span style={{ color: "var(--text-muted)" }}>Price per seat</span><span className="booking-summary-val">{formatEur(showtime.price)}</span></div>
                  <div className="booking-summary-row total"><span>Total</span><span style={{ color: "var(--rose)" }}>{formatEur(totalPrice)}</span></div>
                </div>
              )}
              <button className="btn-primary" style={{ width: "100%", marginTop: "1.25rem", padding: "0.75rem" }} onClick={lockSeats} disabled={loading || selectedSeats.length === 0}>
                {loading ? "Locking..." : `Lock ${selectedSeats.length} seat${selectedSeats.length !== 1 ? "s" : ""} →`}
              </button>
            </>
          )}
          {step === "confirm" && (
            <>
              {timerSec > 0 ? (
                <div className="lock-timer"><span>⏱</span><span>Seats reserved for <strong>{Math.floor(timerSec / 60)}:{String(timerSec % 60).padStart(2, "0")}</strong> — complete your booking</span></div>
              ) : (
                <div className="lock-timer" style={{ borderColor: "rgba(204,102,102,0.4)", background: "rgba(204,102,102,0.1)", color: "#cc6666" }}><span>⚠️</span><span>Seat reservation expired. Go back and re-select.</span></div>
              )}
              <div style={{ background: "rgba(204,139,134,0.06)", borderRadius: 12, padding: "1.25rem", marginBottom: "1.25rem" }}>
                <div style={{ fontFamily: "'Playfair Display', serif", fontSize: "1.15rem", color: "var(--cream)", marginBottom: "1rem" }}>{movie.title}</div>
                {[
                  ["Date", fmtDate(showtime.startTime, { weekday: "long", day: "numeric", month: "long" })],
                  ["Time", fmtTime(showtime.startTime)],
                  ["Hall", showtime.hallName],
                  ["Seats", selectedSeats.join(", ")],
                  ["Total", formatEur(totalPrice)],
                ].map(([k, v]) => (
                  <div key={k} className="booking-summary-row"><span style={{ color: "var(--text-muted)" }}>{k}</span><span className="booking-summary-val">{v}</span></div>
                ))}
              </div>
              <div style={{ display: "flex", gap: "0.75rem" }}>
                <button className="btn-outline" style={{ flex: 1, padding: "0.75rem" }} onClick={handleBack}>← Change seats</button>
                <button className="btn-primary" style={{ flex: 2, padding: "0.75rem" }} onClick={confirmBooking} disabled={loading || timerSec === 0}>
                  {loading ? "Confirming..." : "Confirm Booking"}
                </button>
              </div>
            </>
          )}
          {step === "success" && (
            <div style={{ textAlign: "center", padding: "1.5rem 0" }}>
              <div style={{ fontSize: "3.5rem", marginBottom: "1rem" }}>🎟</div>
              <h2 style={{ fontFamily: "'Playfair Display', serif", fontSize: "1.5rem", color: "var(--cream)", marginBottom: "0.75rem" }}>Booking Received!</h2>
              <p style={{ color: "var(--text-muted)", marginBottom: "0.5rem" }}><strong style={{ color: "var(--cream)" }}>{movie.title}</strong></p>
              <p style={{ color: "var(--text-muted)", fontSize: "0.9rem", marginBottom: "0.3rem" }}>Seats: {selectedSeats.join(", ")}</p>
              <p style={{ color: "var(--rose)", fontSize: "0.9rem", marginBottom: "1rem" }}>
                {fmtDate(showtime.startTime, { weekday: "long", day: "numeric", month: "long" })} · {fmtTime(showtime.startTime)}
              </p>
              <div style={{ background: "rgba(212,168,67,0.1)", border: "1px solid rgba(212,168,67,0.25)", borderRadius: "10px", padding: "1rem", marginBottom: "1.5rem", fontSize: "0.88rem", color: "#d4a843" }}>
                ⚠️ Your booking is <strong>pending payment</strong>. Please complete the payment in <em>My Bookings</em> to confirm your reservation.
              </div>
              <button className="btn-primary" style={{ padding: "0.75rem 2rem" }} onClick={handleClose}>Done</button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

// ─── PAYMENT MODAL ───
function PaymentModal({ booking, token, toast, onClose, onSuccess }) {
  const [method, setMethod] = useState("");
  const [loading, setLoading] = useState(false);

  const pay = async () => {
    if (!method) return toast("Please select a payment method", "error");
    setLoading(true);
    try {
      const r = await authFetch(`${API}/payments`, token, {
        method: "POST",
        body: JSON.stringify({
          userEmail: booking.userEmail,
          movieTitle: booking.movieTitle,
          hallName: booking.hallName,
          showtimeStartTime: booking.showtimeStart,
          method
        })
      });
      if (r.ok || r.status === 201) {
        toast("Payment successful! Confirmation email sent. 🎬", "success");
        onSuccess();
        onClose();
      } else {
        const err = await safeJson(r);
        toast(err.message || "Payment failed", "error");
      }
    } catch { toast("Connection error", "error"); }
    setLoading(false);
  };

  const total = formatEur(booking.totalPrice);

  return (
    <div className="modal-overlay" onClick={e => e.target === e.currentTarget && onClose()}>
      <div className="modal">
        <div className="modal-header">
          <div className="modal-title">Pay for Booking</div>
          <button className="modal-close" onClick={onClose}>×</button>
        </div>
        <div className="modal-body">
          <div style={{ fontFamily: "'Playfair Display', serif", fontSize: "1.1rem", color: "var(--cream)", marginBottom: "0.5rem" }}>{booking.movieTitle}</div>
          <div style={{ fontSize: "0.85rem", color: "var(--text-muted)", marginBottom: "1.25rem" }}>
            {booking.hallName} · {fmtDate(booking.showtimeStart, { day: "numeric", month: "short" })} {fmtTime(booking.showtimeStart)}<br />
            Seats: {booking.seats?.map(s => s.seatLabel).join(", ")}
          </div>
          <div style={{ fontFamily: "'Playfair Display', serif", fontSize: "1.5rem", color: "var(--rose)", marginBottom: "1.25rem" }}>Total: {total}</div>

          <div style={{ fontSize: "0.8rem", color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.08em", marginBottom: "0.75rem" }}>Select Payment Method</div>
          <div className="method-grid">
            {PAYMENT_METHODS.map(m => (
              <button key={m} className={`method-btn ${method === m ? "selected" : ""}`} onClick={() => setMethod(m)}>
                {m === "CreditCard" ? "💳 Credit Card" : m === "DebitCard" ? "💳 Debit Card" : m === "PayPal" ? "🅿️ PayPal" : "🎫 Voucher"}
              </button>
            ))}
          </div>

          <button className="btn-success" style={{ width: "100%", padding: "0.75rem", marginTop: "1rem" }}
            onClick={pay} disabled={loading || !method}>
            {loading ? "Processing..." : `Pay ${total}`}
          </button>
        </div>
      </div>
    </div>
  );
}

// ─── MY BOOKINGS ───
function MyBookingsPage({ user, token, toast }) {
  const [bookings, setBookings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [payBooking, setPayBooking] = useState(null);
  const [detailBooking, setDetailBooking] = useState(null);

  const load = () => {
    authFetch(`${API}/bookings?userEmail=${encodeURIComponent(user.email)}&pageSize=50`, token)
      .then(r => r.json())
      .then(d => { setBookings(d.items || d); setLoading(false); })
      .catch(() => { setLoading(false); });
  };

  useEffect(() => { load(); }, []);

  const cancel = async (id) => {
    try {
      const r = await authFetch(`${API}/bookings/${id}/cancel`, token, { method: "PATCH" });
      if (r.ok || r.status === 204) { toast("Booking cancelled", "info"); load(); }
      else {
        const e = await safeJson(r);
        if (r.status === 401) toast("Session expired — please log in again.", "error");
        else toast(e.message || "Cannot cancel", "error");
      }
    } catch { toast("Connection error", "error"); }
  };

  return (
    <div className="section">
      <div className="section-header"><h1 className="section-title">My Bookings</h1></div>
      {loading ? <div className="spinner" /> : bookings.length === 0 ? (
        <div className="empty-state"><div className="empty-state-icon">🎟</div><p>No bookings yet</p></div>
      ) : bookings.map(b => (
        <div key={b.id} className="booking-card" onClick={() => setDetailBooking(b)}>
          <div className="booking-card-header">
            <div>
              <div className="booking-movie-title">{b.movieTitle}</div>
              <div style={{ fontSize: "0.82rem", color: "var(--text-muted)", marginTop: "3px" }}>
                {b.hallName} · {fmtDate(b.showtimeStart, { weekday: "short", day: "numeric", month: "short" })} {fmtTime(b.showtimeStart)}
              </div>
            </div>
            <div style={{ display: "flex", flexDirection: "column", alignItems: "flex-end", gap: "0.4rem" }}>
              <span className={`status-badge status-${statusClass(b.status)}`}>{displayStatus(b.status)}</span>
              <span style={{ color: "var(--rose)", fontSize: "0.9rem", fontWeight: 600 }}>{formatEur(b.totalPrice)}</span>
            </div>
          </div>
          <div className="booking-seats">
            {b.seats?.map(s => <span key={s.seatLabel} className="seat-tag">{s.seatLabel}</span>)}
          </div>
          <div style={{ display: "flex", gap: "0.5rem", marginTop: "0.75rem" }} onClick={e => e.stopPropagation()}>
            {/* Pay button — samo za Pending rezervacije */}
            {b.status === "Pending" && (
              <button className="btn-success" style={{ fontSize: "0.8rem", padding: "0.35rem 0.8rem" }}
                onClick={() => setPayBooking(b)}>
                💳 Pay Now
              </button>
            )}
            {/* Cancel — samo za Pending (neplacene) rezervacije */}
            {b.status === "Pending" && (
              <button className="btn-cancel" onClick={() => cancel(b.id)}>Cancel</button>
            )}
            {/* Za placene rezervacije — informacija o refundu */}
            {(b.status === "Confirmed" || b.status === "CheckedIn") && (
              <div className="info-box" style={{ marginTop: 0, padding: "0.35rem 0.8rem" }}>
                To cancel, contact admin for a refund
              </div>
            )}
          </div>
        </div>
      ))}

      {payBooking && (
        <PaymentModal booking={payBooking} token={token} toast={toast}
          onClose={() => setPayBooking(null)} onSuccess={load} />
      )}
      {detailBooking && (
        <BookingDetailModal booking={detailBooking} onClose={() => setDetailBooking(null)} />
      )}
    </div>
  );
}

// ─── BOOKING DETAIL MODAL ───
function BookingDetailModal({ booking, onClose }) {
  return (
    <div className="modal-overlay" onClick={e => e.target === e.currentTarget && onClose()}>
      <div className="modal">
        <div className="modal-header">
          <div className="modal-title">Booking Details</div>
          <button className="modal-close" onClick={onClose}>×</button>
        </div>
        <div className="modal-body">
          <div style={{ fontFamily: "'Playfair Display', serif", fontSize: "1.2rem", color: "var(--cream)", marginBottom: "1.25rem" }}>{booking.movieTitle}</div>
          <div style={{ display: "flex", alignItems: "center", gap: "0.75rem", marginBottom: "1.25rem" }}>
            <span className={`status-badge status-${statusClass(booking.status)}`}>{displayStatus(booking.status)}</span>
            <span style={{ color: "var(--rose)", fontWeight: 600, fontSize: "1rem" }}>{formatEur(booking.totalPrice)}</span>
          </div>
          {[
            ["Hall", booking.hallName],
            ["Date", fmtDate(booking.showtimeStart, { weekday: "long", day: "numeric", month: "long", year: "numeric" })],
            ["Time", fmtTime(booking.showtimeStart)],
            ["Seats", booking.seats?.map(s => s.seatLabel).join(", ")],
            ["Booked on", new Date(booking.createdAt).toLocaleString("en-GB", { timeZone: BELGRADE_TZ, day: "numeric", month: "short", year: "numeric", hour: "2-digit", minute: "2-digit" })],
          ].map(([k, v]) => (
            <div key={k} className="verify-info-row">
              <span className="verify-info-label">{k}</span>
              <span className="verify-info-val">{v}</span>
            </div>
          ))}
          {booking.status === "Pending" && (
            <div style={{ marginTop: "1rem", padding: "0.85rem", background: "rgba(212,168,67,0.1)", border: "1px solid rgba(212,168,67,0.25)", borderRadius: "10px", fontSize: "0.85rem", color: "#d4a843" }}>
              ⚠️ This booking is awaiting payment. Use the <strong>Pay Now</strong> button to confirm your reservation.
            </div>
          )}
          {(booking.status === "Confirmed" || booking.status === "CheckedIn") && (
            <div style={{ marginTop: "1rem", padding: "0.85rem", background: "rgba(125,184,125,0.1)", border: "1px solid rgba(125,184,125,0.25)", borderRadius: "10px", fontSize: "0.85rem", color: "#7db87d" }}>
              ✅ Payment confirmed. Your ticket has been sent to your email.
            </div>
          )}
          <button className="btn-outline" style={{ width: "100%", marginTop: "1.5rem", padding: "0.65rem" }} onClick={onClose}>Close</button>
        </div>
      </div>
    </div>
  );
}

// ─── ADMIN PAGE ───
function AdminPage({ token, toast }) {
  const [tab, setTab] = useState("bookings");
  const [bookings, setBookings] = useState([]);
  const [movies, setMovies] = useState([]);
  const [showtimes, setShowtimes] = useState([]);
  const [payments, setPayments] = useState([]);
  const [halls, setHalls] = useState([]);
  const [loading, setLoading] = useState(true);

  const [showAddMovie, setShowAddMovie] = useState(false);
  const [editMovie, setEditMovie] = useState(null);
  const [showAddShowtime, setShowAddShowtime] = useState(false);
  const [showAddHall, setShowAddHall] = useState(false);

  const [newMovie, setNewMovie] = useState({ title: "", description: "", genre: "Drama", durationMinutes: 120, rating: 7.5 });
  const [newShowtime, setNewShowtime] = useState({ movieTitle: "", hallName: "", startTime: "", price: 10 });
  const [newHall, setNewHall] = useState({ name: "", rows: 8, seatsPerRow: 10 });

  const loadData = () => {
    setLoading(true);
    const h = { Authorization: `Bearer ${token}` };
    Promise.all([
      fetch(`${API}/bookings?pageSize=100`, { headers: h }).then(r => r.json()).catch(() => ({ items: [] })),
      fetch(`${API}/movies?pageSize=100`).then(r => r.json()).catch(() => ({ items: [] })),
      fetch(`${API}/showtimes`).then(r => r.json()).catch(() => []),
      fetch(`${API}/payments`, { headers: h }).then(r => r.json()).catch(() => []),
      fetch(`${API}/halls`, { headers: h }).then(r => r.json()).catch(() => []),
    ]).then(([b, m, s, p, hl]) => {
      setBookings(b.items || b);
      setMovies(m.items || m);
      setShowtimes(Array.isArray(s) ? s : s.items || []);
      setPayments(Array.isArray(p) ? p : []);
      setHalls(Array.isArray(hl) ? hl : []);
      setLoading(false);
    });
  };

  useEffect(() => { loadData(); }, []);

  const addMovie = async (e) => {
    e.preventDefault();
    try {
      const r = await authFetch(`${API}/movies`, token, {
        method: "POST",
        body: JSON.stringify({ ...newMovie, durationMinutes: Number(newMovie.durationMinutes), rating: Number(newMovie.rating) })
      });
      if (r.ok || r.status === 201) {
        toast("Movie added!", "success"); setShowAddMovie(false);
        setNewMovie({ title: "", description: "", genre: "Drama", durationMinutes: 120, rating: 7.5 });
        const m = await fetch(`${API}/movies?pageSize=100`).then(r2 => r2.json()).catch(() => ({ items: movies }));
        setMovies(m.items || m);
      } else { const err = await safeJson(r); toast(r.status === 401 ? "Session expired" : err.message || JSON.stringify(err), "error"); }
    } catch { toast("Network error", "error"); }
  };

  const updateMovie = async (e) => {
    e.preventDefault();
    try {
      const r = await authFetch(`${API}/movies/${editMovie.id}`, token, {
        method: "PUT",
        body: JSON.stringify({ title: editMovie.title, description: editMovie.description, genre: editMovie.genre, durationMinutes: Number(editMovie.durationMinutes), rating: Number(editMovie.rating) })
      });
      if (r.ok || r.status === 204) {
        toast("Movie updated!", "success"); setEditMovie(null);
        const m = await fetch(`${API}/movies?pageSize=100`).then(r2 => r2.json()).catch(() => ({ items: movies }));
        setMovies(m.items || m);
      } else { const err = await safeJson(r); toast(r.status === 401 ? "Session expired" : err.message || "Error updating", "error"); }
    } catch { toast("Network error", "error"); }
  };

  const deleteMovie = async (id) => {
    if (!confirm("Delete this movie?")) return;
    const r = await authFetch(`${API}/movies/${id}`, token, { method: "DELETE" }).catch(() => null);
    if (r?.ok || r?.status === 204) { setMovies(m => m.filter(x => x.id !== id)); toast("Deleted", "info"); }
    else toast("Could not delete", "error");
  };

  const addShowtime = async (e) => {
    e.preventDefault();
    try {
      const startTimeUtc = newShowtime.startTime ? new Date(newShowtime.startTime).toISOString() : null;
      if (!startTimeUtc) { toast("Please enter a valid start time", "error"); return; }
      const r = await authFetch(`${API}/showtimes`, token, {
        method: "POST",
        body: JSON.stringify({ ...newShowtime, startTime: startTimeUtc, price: Number(newShowtime.price) })
      });
      if (r.ok || r.status === 201) {
        toast("Showtime added!", "success"); setShowAddShowtime(false);
        setNewShowtime({ movieTitle: "", hallName: "", startTime: "", price: 10 });
        const s = await fetch(`${API}/showtimes`).then(r2 => r2.json()).catch(() => showtimes);
        setShowtimes(Array.isArray(s) ? s : s.items || []);
      } else { const err = await safeJson(r); toast(r.status === 401 ? "Session expired" : err.message || "Error", "error"); }
    } catch { toast("Connection error", "error"); }
  };

  const deleteShowtime = async (id) => {
    if (!confirm("Delete this showtime?")) return;
    const r = await authFetch(`${API}/showtimes/${id}`, token, { method: "DELETE" }).catch(() => null);
    if (r?.ok || r?.status === 204) { setShowtimes(s => s.filter(x => x.id !== id)); toast("Deleted", "info"); }
    else toast("Could not delete", "error");
  };

  const addHall = async (e) => {
    e.preventDefault();
    try {
      const rows = Number(newHall.rows), seatsPerRow = Number(newHall.seatsPerRow);
      const r = await authFetch(`${API}/halls`, token, {
        method: "POST",
        body: JSON.stringify({ name: newHall.name, capacity: rows * seatsPerRow, rows, seatsPerRow })
      });
      if (r.ok || r.status === 201) {
        toast("Hall created!", "success"); setShowAddHall(false); setNewHall({ name: "", rows: 8, seatsPerRow: 10 });
        const hl = await authFetch(`${API}/halls`, token).then(r2 => r2.json()).catch(() => halls);
        setHalls(Array.isArray(hl) ? hl : []);
      } else { const err = await safeJson(r); toast(r.status === 401 ? "Session expired" : err.message || "Error", "error"); }
    } catch { toast("Connection error", "error"); }
  };

  const deleteHall = async (id) => {
    if (!confirm("Delete this hall? This will also remove all its seats.")) return;
    const r = await authFetch(`${API}/halls/${id}`, token, { method: "DELETE" }).catch(() => null);
    if (r?.ok || r?.status === 204) { setHalls(h => h.filter(x => x.id !== id)); toast("Hall deleted", "info"); }
    else toast("Could not delete hall", "error");
  };

  const refundPayment = async (id) => {
    if (!confirm("Process refund? The booking will be cancelled.")) return;
    const r = await authFetch(`${API}/payments/${id}/refund`, token, { method: "PATCH" }).catch(() => null);
    if (r?.ok || r?.status === 204) {
      toast("Refund processed — booking cancelled", "success");
      loadData();
    } else {
      const err = await safeJson(r).catch(() => ({}));
      toast(err?.message || "Could not process refund", "error");
    }
  };

  const stats = {
    totalBookings: bookings.length,
    confirmed: bookings.filter(b => b.status === "Confirmed" || b.status === "CheckedIn").length,
    pending: bookings.filter(b => b.status === "Pending").length,
    revenue: payments.filter(p => p.status === "Completed").reduce((s, p) => s + Number(p.amount || 0), 0),
    movies: movies.length,
    halls: halls.length,
  };

  return (
    <div className="section">
      <div className="section-header"><h1 className="section-title">Admin Panel</h1></div>
      <div className="stats-grid">
        <div className="stat-card"><div className="stat-label">Total Bookings</div><div className="stat-val">{stats.totalBookings}</div></div>
        <div className="stat-card"><div className="stat-label">Confirmed</div><div className="stat-val stat-accent">{stats.confirmed}</div></div>
        <div className="stat-card"><div className="stat-label">Pending Payment</div><div className="stat-val" style={{ color: "#d4a843" }}>{stats.pending}</div></div>
        <div className="stat-card"><div className="stat-label">Revenue</div><div className="stat-val">€{stats.revenue.toFixed(2)}</div></div>
        <div className="stat-card"><div className="stat-label">Movies</div><div className="stat-val">{stats.movies}</div></div>
        <div className="stat-card"><div className="stat-label">Halls</div><div className="stat-val">{stats.halls}</div></div>
      </div>

      <div className="admin-tabs">
        {["bookings", "movies", "showtimes", "halls", "payments"].map(t => (
          <button key={t} className={`admin-tab ${tab === t ? "active" : ""}`} onClick={() => setTab(t)}>
            {t.charAt(0).toUpperCase() + t.slice(1)}
          </button>
        ))}
      </div>

      {loading ? <div className="spinner" /> : (
        <>
          {tab === "bookings" && (
            <div style={{ overflowX: "auto" }}>
              <table className="data-table">
                <thead><tr><th>Movie</th><th>User</th><th>Hall</th><th>Date</th><th>Seats</th><th>Total</th><th>Status</th></tr></thead>
                <tbody>
                  {bookings.map(b => (
                    <tr key={b.id}>
                      <td style={{ color: "var(--cream)", fontWeight: 500 }}>{b.movieTitle}</td>
                      <td><div>{b.userFullName}</div><div style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>{b.userEmail}</div></td>
                      <td>{b.hallName}</td>
                      <td style={{ fontSize: "0.82rem" }}>{fmtDate(b.showtimeStart, { day: "numeric", month: "short" })}<br /><span style={{ color: "var(--text-muted)" }}>{fmtTime(b.showtimeStart)}</span></td>
                      <td>{b.seats?.map(s => s.seatLabel).join(", ")}</td>
                      <td style={{ color: "var(--rose)", fontWeight: 600 }}>{formatEur(b.totalPrice)}</td>
                      <td><span className={`status-badge status-${statusClass(b.status)}`}>{displayStatus(b.status)}</span></td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {tab === "movies" && (
            <>
              <button className="btn-primary" style={{ marginBottom: "1.5rem" }} onClick={() => { setShowAddMovie(!showAddMovie); setEditMovie(null); }}>+ Add Movie</button>
              {showAddMovie && (
                <div className="add-panel">
                  <div className="add-panel-title">New Movie</div>
                  <form onSubmit={addMovie}>
                    <div className="form-row">
                      <div className="form-group"><label className="form-label">Title</label><input className="form-input" value={newMovie.title} onChange={e => setNewMovie({ ...newMovie, title: e.target.value })} required /></div>
                      <div className="form-group"><label className="form-label">Genre</label>
                        <select className="form-input" value={newMovie.genre} onChange={e => setNewMovie({ ...newMovie, genre: e.target.value })}>
                          {GENRES.filter(g => g !== "All").map(g => <option key={g}>{g}</option>)}
                        </select>
                      </div>
                    </div>
                    <div className="form-group"><label className="form-label">Description</label><textarea className="form-input" rows={2} value={newMovie.description} onChange={e => setNewMovie({ ...newMovie, description: e.target.value })} /></div>
                    <div className="form-row">
                      <div className="form-group"><label className="form-label">Duration (min)</label><input type="number" className="form-input" value={newMovie.durationMinutes} onChange={e => setNewMovie({ ...newMovie, durationMinutes: e.target.value })} /></div>
                      <div className="form-group"><label className="form-label">Rating (0–10)</label><input type="number" step="0.1" min="0" max="10" className="form-input" value={newMovie.rating} onChange={e => setNewMovie({ ...newMovie, rating: e.target.value })} /></div>
                    </div>
                    <button type="submit" className="btn-primary">Save Movie</button>
                  </form>
                </div>
              )}
              {editMovie && (
                <div className="add-panel" style={{ borderColor: "rgba(204,139,134,0.4)" }}>
                  <div className="add-panel-title">Edit Movie — {editMovie.title}</div>
                  <form onSubmit={updateMovie}>
                    <div className="form-row">
                      <div className="form-group"><label className="form-label">Title</label><input className="form-input" value={editMovie.title} onChange={e => setEditMovie({ ...editMovie, title: e.target.value })} required /></div>
                      <div className="form-group"><label className="form-label">Genre</label>
                        <select className="form-input" value={editMovie.genre} onChange={e => setEditMovie({ ...editMovie, genre: e.target.value })}>
                          {GENRES.filter(g => g !== "All").map(g => <option key={g}>{g}</option>)}
                        </select>
                      </div>
                    </div>
                    <div className="form-group"><label className="form-label">Description</label><textarea className="form-input" rows={2} value={editMovie.description} onChange={e => setEditMovie({ ...editMovie, description: e.target.value })} /></div>
                    <div className="form-row">
                      <div className="form-group"><label className="form-label">Duration (min)</label><input type="number" className="form-input" value={editMovie.durationMinutes} onChange={e => setEditMovie({ ...editMovie, durationMinutes: e.target.value })} /></div>
                      <div className="form-group"><label className="form-label">Rating (0–10)</label><input type="number" step="0.1" min="0" max="10" className="form-input" value={editMovie.rating} onChange={e => setEditMovie({ ...editMovie, rating: e.target.value })} /></div>
                    </div>
                    <div style={{ display: "flex", gap: "0.75rem" }}>
                      <button type="submit" className="btn-primary">Update Movie</button>
                      <button type="button" className="btn-outline" onClick={() => setEditMovie(null)}>Cancel</button>
                    </div>
                  </form>
                </div>
              )}
              <table className="data-table">
                <thead><tr><th>Title</th><th>Genre</th><th>Duration</th><th>Rating</th><th>Showtimes</th><th></th></tr></thead>
                <tbody>
                  {movies.map(m => (
                    <tr key={m.id}>
                      <td style={{ color: "var(--cream)", fontWeight: 500 }}><span style={{ marginRight: 8 }}>{getEmoji(m.genre)}</span>{m.title}</td>
                      <td>{m.genre}</td>
                      <td>{m.durationMinutes} min</td>
                      <td><span className="status-badge" style={{ background: "rgba(204,139,134,0.1)", color: "var(--rose)" }}>★ {Number(m.rating).toFixed(1)}</span></td>
                      <td>{m.showtimeCount}</td>
                      <td style={{ display: "flex", gap: "0.5rem" }}>
                        <button className="btn-outline" style={{ fontSize: "0.78rem", padding: "0.3rem 0.7rem" }}
                          onClick={() => { setEditMovie({ ...m }); setShowAddMovie(false); }}>Edit</button>
                        <button className="btn-cancel" onClick={() => deleteMovie(m.id)}>Delete</button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </>
          )}

          {tab === "showtimes" && (
            <>
              <button className="btn-primary" style={{ marginBottom: "1.5rem" }} onClick={() => setShowAddShowtime(!showAddShowtime)}>+ Add Showtime</button>
              {showAddShowtime && (
                <div className="add-panel">
                  <div className="add-panel-title">New Showtime</div>
                  <form onSubmit={addShowtime}>
                    <div className="form-row">
                      <div className="form-group"><label className="form-label">Movie</label>
                        <select className="form-input" value={newShowtime.movieTitle} onChange={e => setNewShowtime({ ...newShowtime, movieTitle: e.target.value })} required>
                          <option value="">Select movie</option>
                          {movies.map(m => <option key={m.id} value={m.title}>{m.title}</option>)}
                        </select>
                      </div>
                      <div className="form-group"><label className="form-label">Hall</label>
                        <select className="form-input" value={newShowtime.hallName} onChange={e => setNewShowtime({ ...newShowtime, hallName: e.target.value })} required>
                          <option value="">Select hall</option>
                          {halls.map(h => <option key={h.id} value={h.name}>{h.name} ({h.capacity} seats)</option>)}
                        </select>
                      </div>
                    </div>
                    <div className="form-row">
                      <div className="form-group"><label className="form-label">Start Time (Belgrade)</label><input type="datetime-local" className="form-input" value={newShowtime.startTime} onChange={e => setNewShowtime({ ...newShowtime, startTime: e.target.value })} required /></div>
                      <div className="form-group"><label className="form-label">Price (EUR)</label><input type="number" step="0.01" min="0" className="form-input" value={newShowtime.price} onChange={e => setNewShowtime({ ...newShowtime, price: e.target.value })} /></div>
                    </div>
                    <button type="submit" className="btn-primary">Save Showtime</button>
                  </form>
                </div>
              )}
              <table className="data-table">
                <thead><tr><th>Movie</th><th>Hall</th><th>Date & Time</th><th>Price</th><th>Seats left</th><th></th></tr></thead>
                <tbody>
                  {showtimes.map(s => (
                    <tr key={s.id}>
                      <td style={{ color: "var(--cream)", fontWeight: 500 }}>{s.movieTitle}</td>
                      <td>{s.hallName}</td>
                      <td style={{ fontSize: "0.82rem" }}>{fmtDate(s.startTime, { day: "numeric", month: "short", year: "numeric" })}<br /><span style={{ color: "var(--text-muted)" }}>{fmtTime(s.startTime)}</span></td>
                      <td style={{ color: "var(--rose)" }}>{formatEur(s.price)}</td>
                      <td><span className={s.availableSeats > 10 ? "avail-good" : "avail-low"}>{s.availableSeats}</span></td>
                      <td><button className="btn-cancel" onClick={() => deleteShowtime(s.id)}>Delete</button></td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </>
          )}

          {tab === "halls" && (
            <>
              <button className="btn-primary" style={{ marginBottom: "1.5rem" }} onClick={() => setShowAddHall(!showAddHall)}>+ Add Hall</button>
              {showAddHall && (
                <div className="add-panel">
                  <div className="add-panel-title">New Hall</div>
                  <form onSubmit={addHall}>
                    <div className="form-group"><label className="form-label">Hall Name</label><input className="form-input" value={newHall.name} onChange={e => setNewHall({ ...newHall, name: e.target.value })} required placeholder="e.g. Hall 3" /></div>
                    <div className="form-row">
                      <div className="form-group"><label className="form-label">Rows</label><input type="number" min="1" max="30" className="form-input" value={newHall.rows} onChange={e => setNewHall({ ...newHall, rows: e.target.value })} required /></div>
                      <div className="form-group"><label className="form-label">Seats per Row</label><input type="number" min="1" max="30" className="form-input" value={newHall.seatsPerRow} onChange={e => setNewHall({ ...newHall, seatsPerRow: e.target.value })} required /></div>
                    </div>
                    <div style={{ fontSize: "0.82rem", color: "var(--text-muted)", marginBottom: "1rem" }}>
                      Total capacity: <strong style={{ color: "var(--rose)" }}>{Number(newHall.rows) * Number(newHall.seatsPerRow)} seats</strong>
                    </div>
                    <button type="submit" className="btn-primary">Create Hall</button>
                  </form>
                </div>
              )}
              <table className="data-table">
                <thead><tr><th>Name</th><th>Capacity</th><th>Seats</th><th></th></tr></thead>
                <tbody>
                  {halls.length === 0 ? (
                    <tr><td colSpan={4} style={{ textAlign: "center", color: "var(--text-muted)", padding: "2rem" }}>No halls yet</td></tr>
                  ) : halls.map(h => (
                    <tr key={h.id}>
                      <td style={{ color: "var(--cream)", fontWeight: 500 }}>🏛 {h.name}</td>
                      <td>{h.capacity}</td>
                      <td>{h.seatCount}</td>
                      <td><button className="btn-cancel" onClick={() => deleteHall(h.id)}>Delete</button></td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </>
          )}

          {tab === "payments" && (
            <div style={{ overflowX: "auto" }}>
              {payments.length === 0 ? (
                <div className="empty-state"><div className="empty-state-icon">💳</div><p>No payments yet.</p></div>
              ) : (
                <table className="data-table">
                  <thead><tr><th>ID</th><th>Movie</th><th>User</th><th>Amount</th><th>Method</th><th>Date</th><th>Booking</th><th>Status</th><th></th></tr></thead>
                  <tbody>
                    {payments.map(p => (
                      <tr key={p.id}>
                        <td style={{ color: "var(--text-muted)", fontSize: "0.8rem" }}>#{p.id}</td>
                        <td style={{ color: "var(--cream)", fontWeight: 500 }}>{p.movieTitle}</td>
                        <td><div>{p.userFullName}</div><div style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>{p.userEmail}</div></td>
                        <td style={{ color: "var(--rose)", fontWeight: 600 }}>{formatEur(p.amount)}</td>
                        <td>{p.method}</td>
                        <td style={{ fontSize: "0.82rem" }}>{fmtDate(p.paymentDate, { day: "numeric", month: "short" })}<br /><span style={{ color: "var(--text-muted)" }}>{fmtTime(p.paymentDate)}</span></td>
                        <td><span className={`status-badge status-${statusClass(p.bookingStatus)}`}>{displayStatus(p.bookingStatus)}</span></td>
                        <td>
                          <span className={`status-badge ${p.status === "Completed" ? "status-confirmed" : p.status === "Refunded" ? "status-canceled" : "status-pending"}`}>
                            {p.status}
                          </span>
                        </td>
                        <td>
                          {p.status === "Completed" && (
                            <button className="btn-cancel" onClick={() => refundPayment(p.id)}>Refund</button>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
          )}
        </>
      )}
    </div>
  );
}

// ─── AUTH MODAL ───
function AuthModal({ mode, onClose, onLogin, switchMode, toast }) {
  const [form, setForm] = useState({ firstName: "", lastName: "", email: "", password: "" });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const submit = async (e) => {
    e.preventDefault();
    setLoading(true); setError("");
    try {
      if (mode === "login") {
        const r = await fetch(`${API}/auth/login`, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ email: form.email, password: form.password }) });
        if (r.ok) { const d = await r.json(); onLogin(d.token); }
        else { const e2 = await safeJson(r); setError(e2.message || "Invalid credentials"); }
      } else {
        const r = await fetch(`${API}/auth/register`, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ ...form, role: "User" }) });
        if (r.ok) { toast("Account created — please login", "success"); switchMode("login"); }
        else { const e2 = await safeJson(r); setError(e2.message || JSON.stringify(e2.errors || "Error")); }
      }
    } catch { setError("Cannot connect to server — is the backend running?"); }
    setLoading(false);
  };

  return (
    <div className="modal-overlay" onClick={e => e.target === e.currentTarget && onClose()}>
      <div className="modal">
        <div className="modal-header">
          <div className="modal-title">{mode === "login" ? "Welcome back" : "Create account"}</div>
          <button className="modal-close" onClick={onClose}>×</button>
        </div>
        <div className="modal-body">
          <form onSubmit={submit}>
            {mode === "register" && (
              <div className="form-row">
                <div className="form-group"><label className="form-label">First Name</label><input className="form-input" value={form.firstName} onChange={e => setForm({ ...form, firstName: e.target.value })} required /></div>
                <div className="form-group"><label className="form-label">Last Name</label><input className="form-input" value={form.lastName} onChange={e => setForm({ ...form, lastName: e.target.value })} required /></div>
              </div>
            )}
            <div className="form-group"><label className="form-label">Email</label><input type="email" className="form-input" value={form.email} onChange={e => setForm({ ...form, email: e.target.value })} required /></div>
            <div className="form-group"><label className="form-label">Password</label><input type="password" className="form-input" value={form.password} onChange={e => setForm({ ...form, password: e.target.value })} required /></div>
            {error && <div className="form-error" style={{ marginBottom: "0.75rem" }}>{error}</div>}
            <button type="submit" className="btn-primary" style={{ width: "100%", padding: "0.75rem" }} disabled={loading}>
              {loading ? "Please wait..." : mode === "login" ? "Login" : "Create account"}
            </button>
          </form>
          <div style={{ textAlign: "center", marginTop: "1.25rem", fontSize: "0.85rem", color: "var(--text-muted)" }}>
            {mode === "login"
              ? <>No account? <button className="section-link" onClick={() => switchMode("register")}>Register</button></>
              : <>Already a member? <button className="section-link" onClick={() => switchMode("login")}>Login</button></>}
          </div>
        </div>
      </div>
    </div>
  );
}
