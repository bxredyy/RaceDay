"""Draw the implemented six-entity RaceDay ERD. Requires Pillow for regeneration."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
image = Image.new('RGB', (1600, 1110), '#ffffff')
draw = ImageDraw.Draw(image)
font_root = Path('/usr/share/fonts/truetype/dejavu')

def font(size, bold=False):
    name = 'DejaVuSans-Bold.ttf' if bold else 'DejaVuSans.ttf'
    try:
        return ImageFont.truetype(str(font_root / name), size)
    except OSError:
        return ImageFont.load_default()

normal = font(18)
small = font(16)
heading = font(23, True)

def text(x, y, value, face=normal, fill='#243447'):
    draw.text((x, y), value, font=face, fill=fill)

def box(x, y, width, name, rows):
    height = 52 + 27 * len(rows) + 13
    draw.rounded_rectangle((x, y, x+width, y+height), radius=10, fill='#f5f8fb', outline='#718096', width=2)
    draw.rounded_rectangle((x, y, x+width, y+47), radius=10, fill='#153c53')
    draw.rectangle((x, y+30, x+width, y+47), fill='#153c53')
    text(x+16, y+9, name, heading, '#ffffff')
    for i, row in enumerate(rows):
        text(x+15, y+56+27*i, row, small)
    return height

def edge(points, labels):
    draw.line(points, fill='#52677b', width=3)
    for x,y,label in labels:
        bounds = draw.textbbox((x,y),label,font=small)
        draw.rectangle((bounds[0]-5,bounds[1]-3,bounds[2]+5,bounds[3]+3),fill='white')
        text(x,y,label,small)

text(48, 20, 'RaceDay implemented entity relationship diagram', font(30, True))
text(48, 64, 'Part 2 amendment: Events.DistanceKm added. Original ERD preserved separately.', normal)

box(60,160,390,'Users',[
 'PK  UserId : int', 'FullName : nvarchar(100)', 'UQ  Email : nvarchar(254)',
 'PasswordHash : nvarchar(500)', 'Role : nvarchar(20)', 'PhoneNumber : nvarchar(30)',
 'ProfilePictureUrl : nvarchar(2048)?', 'CreatedAt : datetime2'])
box(590,160,410,'Events',[
 'PK  EventId : int', 'FK  OrganiserId : int -> Users', 'Name : nvarchar(150)',
 'Description : nvarchar(2000)', 'EventDate : datetime2', 'Location : nvarchar(200)',
 'DistanceKm : decimal(8,2)', 'EventType : nvarchar(10)',
 'BannerImageUrl : nvarchar(2048)?', 'CreatedAt : datetime2'])
box(1140,160,390,'AuditLogs',[
 'PK  LogId : int', 'FK  UserId : int -> Users', 'Action : nvarchar(500)', 'Timestamp : datetime2'])
box(60,650,410,'Categories',[
 'PK  CategoryId : int', 'FK  EventId : int -> Events', 'CategoryName : nvarchar(100)',
 'Description : nvarchar(1000)', 'DistanceKm : decimal(8,2)', 'Fee : decimal(10,2)'])
box(590,650,410,'Enrolments',[
 'PK  EnrolmentId : int', 'FK  ParticipantId : int -> Users', 'FK  EventId : int -> Events',
 'FK  CategoryId + EventId -> Categories', 'EnrolmentDate : datetime2', 'Status : nvarchar(20)'])
box(1140,650,390,'Results',[
 'PK  ResultId : int', 'FK/UQ  EnrolmentId : int', 'FinishTime : time',
 'OverallPosition : int', 'CategoryPosition : int', 'RecordedAt : datetime2'])

edge([(450,245),(590,245)],[(459,219,'1'),(524,219,'0..*')])
edge([(350,160),(350,115),(1320,115),(1320,160)],[(360,121,'1'),(1330,126,'0..*')])
edge([(590,400),(535,400),(535,575),(265,575),(265,650)],[(546,414,'1'),(280,606,'0..*')])
edge([(795,495),(795,650)],[(811,510,'1'),(811,603,'0..*')])
edge([(450,370),(500,370),(500,630),(570,630),(570,906),(700,906),(700,877)],[(461,378,'1'),(712,884,'0..*')])
edge([(470,758),(590,758)],[(481,728,'1'),(535,728,'0..*')])
edge([(1000,758),(1140,758)],[(1012,728,'1'),(1071,728,'0..1')])

text(60, 956, 'Keys and safeguards', font(21,True))
text(60, 993, 'PK primary key   FK foreign key   UQ unique   ? optional field', small)
text(60, 1023, 'Unique pairs: Enrolments(ParticipantId, EventId); Categories(EventId, CategoryName).', small)
text(60, 1053, 'Categories has alternate key (CategoryId, EventId). Composite FK enforces the selected event/category pair.', small)
image.save(ROOT/'docs/erd.png')
print(ROOT/'docs/erd.png')
