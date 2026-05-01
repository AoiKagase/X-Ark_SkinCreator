using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SkinCreator.Json;

// NewSkinSystem の JSON 構造に対応する POCO モデル群（SkinCreator 独自定義）

class SkinJsonRoot
{
	[JsonPropertyName("version")]  public string Version  { get; set; } = "2.0";
	[JsonPropertyName("meta")]     public SkinMetaJson?     Meta     { get; set; }
	[JsonPropertyName("settings")] public SkinSettingsJson? Settings { get; set; }
	[JsonPropertyName("images")]   public Dictionary<string, string>? Images { get; set; }
	[JsonPropertyName("mainForm")] public MainFormJson?  MainForm { get; set; }
	[JsonPropertyName("subForms")] public Dictionary<string, SubFormJson>? SubForms { get; set; }
}

class SkinMetaJson
{
	[JsonPropertyName("name")]        public string? Name        { get; set; }
	[JsonPropertyName("author")]      public string? Author      { get; set; }
	[JsonPropertyName("description")] public string? Description { get; set; }
}

class SkinSettingsJson
{
	[JsonPropertyName("transparentKey")] public string? TransparentKey { get; set; }
}

class LocationJson
{
	[JsonPropertyName("x")] public int X { get; set; }
	[JsonPropertyName("y")] public int Y { get; set; }
	[JsonPropertyName("w")] public int W { get; set; }
	[JsonPropertyName("h")] public int H { get; set; }
}

class SpriteRectJson
{
	[JsonPropertyName("imageKey")] public string? ImageKey { get; set; }
	[JsonPropertyName("x")] public int X { get; set; }
	[JsonPropertyName("y")] public int Y { get; set; }
	[JsonPropertyName("w")] public int W { get; set; }
	[JsonPropertyName("h")] public int H { get; set; }
}

class ButtonJson
{
	[JsonPropertyName("up")]       public SpriteRectJson? Up       { get; set; }
	[JsonPropertyName("down")]     public SpriteRectJson? Down     { get; set; }
	[JsonPropertyName("hover")]    public SpriteRectJson? Hover    { get; set; }
	[JsonPropertyName("disabled")] public SpriteRectJson? Disabled { get; set; }
	[JsonPropertyName("optional")] public SpriteRectJson? Optional { get; set; }
	[JsonPropertyName("location")] public LocationJson?   Location { get; set; }
	[JsonPropertyName("isDisabled")] public bool          IsDisabled { get; set; }
}

class SliderJson
{
	[JsonPropertyName("src")]         public SpriteRectJson? Src         { get; set; }
	[JsonPropertyName("location")]    public LocationJson?   Location    { get; set; }
	[JsonPropertyName("min")]         public int             Min         { get; set; }
	[JsonPropertyName("max")]         public int             Max         { get; set; }
	[JsonPropertyName("orientation")] public string?         Orientation { get; set; }
	[JsonPropertyName("isDisabled")]  public bool            IsDisabled  { get; set; }
}

class LabelJson
{
	[JsonPropertyName("location")]     public LocationJson? Location     { get; set; }
	[JsonPropertyName("font")]         public string?       Font         { get; set; }
	[JsonPropertyName("size")]         public int           Size         { get; set; }
	[JsonPropertyName("bold")]         public bool          Bold         { get; set; }
	[JsonPropertyName("italic")]       public bool          Italic       { get; set; }
	[JsonPropertyName("foreColor")]    public string?       ForeColor    { get; set; }
	[JsonPropertyName("backColor")]    public string?       BackColor    { get; set; }
	[JsonPropertyName("align")]        public string?       Align        { get; set; }
	[JsonPropertyName("scrollEnable")] public bool          ScrollEnable { get; set; }
	[JsonPropertyName("scrollVector")] public int           ScrollVector { get; set; } = 3;
	[JsonPropertyName("interval")]     public int           Interval     { get; set; }
	[JsonPropertyName("additionalValue")] public string?    AdditionalValue { get; set; }
	[JsonPropertyName("isDisabled")]   public bool          IsDisabled   { get; set; }
}

class SpectrumJson
{
	[JsonPropertyName("src")]        public SpriteRectJson? Src        { get; set; }
	[JsonPropertyName("color")]      public string?         Color      { get; set; }
	[JsonPropertyName("location")]   public LocationJson?   Location   { get; set; }
	[JsonPropertyName("waveColorL")] public string?         WaveColorL { get; set; }
	[JsonPropertyName("waveColorR")] public string?         WaveColorR { get; set; }
	[JsonPropertyName("isDisabled")] public bool            IsDisabled { get; set; }
}

class WaveAreaJson
{
	[JsonPropertyName("target")]       public string? Target       { get; set; }
	[JsonPropertyName("mode")]         public string? Mode         { get; set; }
	[JsonPropertyName("exponent")]     public float   Exponent     { get; set; }
	[JsonPropertyName("colorL")]       public string? ColorL       { get; set; }
	[JsonPropertyName("colorR")]       public string? ColorR       { get; set; }
	[JsonPropertyName("colorMix")]     public string? ColorMix     { get; set; }
	[JsonPropertyName("colorPlayed")]  public string? ColorPlayed  { get; set; }
	[JsonPropertyName("colorUnplayed")]public string? ColorUnplayed{ get; set; }
	[JsonPropertyName("location")]     public LocationJson? Location { get; set; }
	[JsonPropertyName("isDisabled")]   public bool          IsDisabled { get; set; }
}

class PictureJson
{
	[JsonPropertyName("src")]         public SpriteRectJson? Src         { get; set; }
	[JsonPropertyName("color")]       public string?         Color       { get; set; }
	[JsonPropertyName("location")]    public LocationJson?   Location    { get; set; }
	[JsonPropertyName("borderColor")] public string?         BorderColor { get; set; }
	[JsonPropertyName("borderWidth")] public int             BorderWidth { get; set; }
	[JsonPropertyName("cornerRadius")] public int            CornerRadius { get; set; }
	[JsonPropertyName("isDisabled")]  public bool            IsDisabled  { get; set; }
}

class GridJson
{
	[JsonPropertyName("location")]        public LocationJson? Location        { get; set; }
	[JsonPropertyName("backColor")]       public string?       BackColor       { get; set; }
	[JsonPropertyName("foreColor")]       public string?       ForeColor       { get; set; }
	[JsonPropertyName("lineColor")]       public string?       LineColor       { get; set; }
	[JsonPropertyName("headerBackColor")] public string?       HeaderBackColor { get; set; }
	[JsonPropertyName("headerForeColor")] public string?       HeaderForeColor { get; set; }
	[JsonPropertyName("headerLineColor")] public string?       HeaderLineColor { get; set; }
	[JsonPropertyName("isDisabled")]      public bool          IsDisabled      { get; set; }
}

class MainFormJson
{
	[JsonPropertyName("src")]      public SpriteRectJson? Src      { get; set; }
	[JsonPropertyName("location")] public LocationJson?   Location { get; set; }
	[JsonPropertyName("buttons")]  public Dictionary<string, ButtonJson>?  Buttons  { get; set; }
	[JsonPropertyName("sliders")]  public Dictionary<string, SliderJson>?  Sliders  { get; set; }
	[JsonPropertyName("labels")]   public Dictionary<string, LabelJson>?   Labels   { get; set; }
	[JsonPropertyName("spectrum")] public SpectrumJson?   Spectrum { get; set; }
	[JsonPropertyName("waveArea")] public WaveAreaJson?   WaveArea { get; set; }
	[JsonPropertyName("pictures")] public Dictionary<string, PictureJson>? Pictures { get; set; }
}

class SubFormJson
{
	[JsonPropertyName("src")]       public SpriteRectJson? Src       { get; set; }
	[JsonPropertyName("location")]  public LocationJson?   Location  { get; set; }
	[JsonPropertyName("offset")]    public LocationJson?   Offset    { get; set; }
	[JsonPropertyName("magnetic")]  public bool            Magnetic  { get; set; }
	[JsonPropertyName("backColor")] public string?         BackColor { get; set; }
	[JsonPropertyName("foreColor")] public string?         ForeColor { get; set; }
	[JsonPropertyName("font")]      public string?         Font      { get; set; }
	[JsonPropertyName("fontSize")]  public int             FontSize  { get; set; }
	[JsonPropertyName("buttons")]   public Dictionary<string, ButtonJson>? Buttons { get; set; }
	[JsonPropertyName("sliders")]   public Dictionary<string, SliderJson>? Sliders { get; set; }
	[JsonPropertyName("labels")]    public Dictionary<string, LabelJson>?  Labels  { get; set; }
	[JsonPropertyName("grids")]     public Dictionary<string, GridJson>?   Grids   { get; set; }
	[JsonPropertyName("pictures")]  public Dictionary<string, PictureJson>? Pictures { get; set; }
}
