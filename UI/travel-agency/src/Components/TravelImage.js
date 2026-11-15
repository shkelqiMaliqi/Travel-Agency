import React from "react";

const placeholder = `${process.env.PUBLIC_URL || ""}/images/travel-placeholder.svg`;

function resize(source, width) {
  try {
    const url = new URL(source);
    if (url.hostname === "via.placeholder.com") return placeholder;
    if (url.hostname === "images.unsplash.com") {
      url.searchParams.set("auto", "format");
      url.searchParams.set("fit", "crop");
      url.searchParams.set("w", String(width));
      url.searchParams.set("q", "75");
      return url.toString();
    }
  } catch { return source || placeholder; }
  return source;
}

export default function TravelImage({ src, alt, width = 600, height = 330, sizes, ...props }) {
  const isUnsplash = /^https:\/\/images\.unsplash\.com\//.test(src || "");
  return <img
    loading="lazy"
    decoding="async"
    {...props}
    src={resize(src, 960)}
    srcSet={isUnsplash ? [320, 640, 960].map((size) => `${resize(src, size)} ${size}w`).join(", ") : undefined}
    sizes={isUnsplash ? sizes || "(max-width: 768px) 100vw, 50vw" : undefined}
    alt={alt}
    width={width}
    height={height}
    onError={(event) => {
      event.currentTarget.onerror = null;
      event.currentTarget.removeAttribute("srcset");
      if (!event.currentTarget.src.endsWith("/images/travel-placeholder.svg")) event.currentTarget.src = placeholder;
    }}
  />;
}
