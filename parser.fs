\ Basic parsing utilities
: match-char ( addr len c -- addr' len' flag )
  >r 2dup 0= swap 0= or if 
    ." Error: Unexpected end of input in match-char!" cr abort 
  then
  over c@ r> = dup if rot char+ rot 1- rot else 2drop then ;

\ HTML tag utilities
: tag-start? ( addr len -- addr' len' flag )
  ." Checking for tag start... " 2dup type cr
  [char] < match-char ;

: tag-end? ( addr len -- addr' len' flag )
  ." Checking for tag end... " 2dup type cr
  [char] > match-char ;

: is-name-char ( c -- flag )
    dup bl <> swap          \ Not a space
    dup [char] > <> swap    \ Not >
    dup [char] < <> swap    \ Not <
    dup [char] / <> swap    \ Not /
    drop and and and ;      \ Combine all conditions

: find-name-end ( addr len -- addr len pos )
  0 >r                    \ Initialize counter
  begin
    2dup r@ - 0> while    \ While we haven't reached the end
    2dup r@ + c@          \ Get current char
    is-name-char 0= if    \ If not a name char
      r> exit            \ Return current position
    then
    r> 1+ >r             \ Increment counter
  repeat
  r> ;

: token-name ( addr len -- addr' len' name-addr name-len )
  ." Extracting token name... " 2dup type cr
  2dup 0= swap 0= or if 
    ." Error: Empty input in token-name!" cr abort 
  then
  over c@ bl = if
    ." Error: Unexpected space at start of token name!" cr abort 
  then
  
  2dup find-name-end     \ Find end of name
  >r 2dup drop r> \ Just use the new length
  ;

\ Parsing combinators
: parse-tag ( addr len -- addr' len' name-addr name-len )
  ." Parsing tag... " 2dup type cr
  tag-start? 0= if ." Error: Missing < in tag!" cr abort then
  token-name 
  2swap 2dup tag-end? 0= if ." Error: Missing > in tag!" cr abort then
  2swap
  ;

: find-content-end ( addr len -- addr len pos )
  0 >r                    \ Initialize counter
  begin
    2dup r@ - 0> while    \ While we haven't reached the end
    2dup r@ + c@          \ Get current char
    [char] < = if         \ If we found <
      r> exit            \ Return current position
    then
    r> 1+ >r             \ Increment counter
  repeat
  r> ;

: parse-content ( addr len -- addr' len' content-addr content-len )
  ." Parsing content... " 2dup type cr
  2dup 0= swap 0= or if 0 0 0 0 exit then
  over c@ [char] < = if 0 0 0 0 exit then
  
  2dup find-content-end  \ Find end of content
  >r                     \ Save position
  
  \ Calculate content length and remaining string
  2dup r@ /string       \ Get remaining string after content
  2swap                 \ Bring original string to top
  r> 2dup >r           \ Duplicate position for later
  /string drop         \ Get content start
  r>                   \ Get content length
  2swap ;              \ Put remainder on top

: parse-html ( addr len -- )
  ." Starting HTML parsing..." cr
  begin
    2dup 0= swap 0= or if 
      2drop ." Done parsing HTML!" cr exit 
    then
    parse-tag 2>r 
    ." Tag found: " 2r@ type cr
    2r>
    parse-content 2dup if ." Content: " type cr then
  again ;

\ Test cases
: test-html ( -- )
  s" <html>Hello, World!</html>" 
  2dup ." Input: " type cr 
  parse-html ;

: test-nested ( -- )
  s" <div><p>Nested content</p></div>"
  2dup ." Input: " type cr
  parse-html ;

\ Run test
test-html
