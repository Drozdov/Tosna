/** Simplified XML parser. Looks for tags. All tags validation (closing slashes, etc) is done
 when tree analyzing. This grammar just produces tags without location validation */
parser grammar XMLParser;

options { tokenVocab=XMLLexer; }

document    :  misc* (element content misc*)* EOF ;

content     :   chardata?
                ((reference | COMMENT) chardata?)* ;

element     :   OPEN opening Name? attribute* closing CLOSE?;

opening     :   (SLASH|QUESTION)?;

closing     :   (SLASH|QUESTION)?;

reference   :   EntityRef | CharRef ;

attribute   :   Name '=' STRING ; // Our STRING is AttValue in spec

chardata    :   TEXT | SEA_WS ;

misc        :   COMMENT | SEA_WS ;