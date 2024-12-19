import React from 'react';
import { Link } from 'react-router-dom';
//import { ReactComponent as MenuIcon } from './menu.svg'; 

export default function Navbar() {
    return (
        <nav className="navbar navbar-expand-lg navbar-light" style={{ background: 'linear-gradient(20deg, #ccffcc, lightblue)' }}>
            <div className="container-fluid">
                <Link to="/" className="navbar-brand site-title">
                    Travel Agency
                </Link>
                <button className="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav" aria-controls="navbarNav" aria-expanded="false" aria-label="Toggle navigation">
                    
                </button>
                <div className="collapse navbar-collapse justify-content-center" id="navbarNav">
                    <ul className="navbar-nav">
                        <CustomLink to="/Aboutus">About Us</CustomLink>
                        <CustomLink to="/Contactus">Contact Us</CustomLink>
                    </ul>
                </div>
                <Link to="/LoginPage" className="nav-link site-profile">
                    Profile
                </Link>
            </div>
        </nav>
    );
}

function CustomLink({ to, children, ...props }) {
    const path = window.location.pathname;

    return (
        <li className={`nav-item ${path === to ? 'active' : ''}`}>
            <Link to={to} className="nav-link" {...props}>
                {children}
            </Link>
        </li>
    );
}

